namespace NetDaemon.Client.Internal;

internal class HomeAssistantConnection : IHomeAssistantConnection, IHomeAssistantHassMessages
{
    #region -- private declarations -

    private readonly ILogger<IHomeAssistantConnection> _logger;
    private readonly IWebSocketClientTransportPipeline _transportPipeline;
    private readonly IHomeAssistantApiManager _apiManager;
    private readonly ResultMessageHandler _resultMessageHandler;
    private readonly CancellationTokenSource _internalCancelSource = new();

    private readonly Subject<HassMessage> _hassMessageSubject = new();
    private readonly Task _handleNewMessagesTask;

    private const int WaitForResultTimeout = 20000;

    private readonly SemaphoreSlim _messageIdSemaphore = new(1, 1);
    private int _messageId = 1;
    private readonly AsyncLazy<IObservable<HassEvent>> _lazyAllEventsObservable;

    #endregion

    /// <summary>
    ///     Default constructor
    /// </summary>
    /// <param name="logger">A logger instance</param>
    /// <param name="pipeline">The pipeline to use for websocket communication</param>
    /// <param name="apiManager">The api manager</param>
    public HomeAssistantConnection(
        ILogger<IHomeAssistantConnection> logger,
        IWebSocketClientTransportPipeline pipeline,
        IHomeAssistantApiManager apiManager
    )
    {
        _transportPipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _apiManager = apiManager;
        _logger = logger;

        _resultMessageHandler = new ResultMessageHandler(_logger, TimeProvider.System);

        // We lazily cache same observable for all events. There are no reason we should use multiple subscriptions
        // to all events. If people wants that they can provide a "*" type and get the same thing
        _lazyAllEventsObservable = new AsyncLazy<IObservable<HassEvent>>(async Task<IObservable<HassEvent>>() =>
            await SubscribeToHomeAssistantEventsInternalAsync(null, _internalCancelSource.Token));

        if (_transportPipeline.WebSocketState != WebSocketState.Open)
            throw new ApplicationException(
                $"Expected WebSocket state 'Open' got '{_transportPipeline.WebSocketState}'");

        _handleNewMessagesTask = Task.Factory.StartNew(async () => await HandleNewMessages().ConfigureAwait(false),
            TaskCreationOptions.LongRunning);
    }

    public async Task<IObservable<HassEvent>> SubscribeToHomeAssistantEventsAsync(string? eventType,
        CancellationToken cancelToken)
    {
        // When subscribe all events, optimize using the same IObservable<HassEvent> if we subscribe multiple times
        if (string.IsNullOrEmpty(eventType))
            return await _lazyAllEventsObservable.Value;

        return await SubscribeToHomeAssistantEventsInternalAsync(eventType, cancelToken);
    }

    private async Task<IObservable<HassEvent>> SubscribeToHomeAssistantEventsInternalAsync(string? eventType,
        CancellationToken cancelToken)
    {
        var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancelToken,
            _internalCancelSource.Token
        );

        var result = await SendCommandAndReturnHassMessageResponseAsync(new SubscribeEventCommand(),
            combinedTokenSource.Token).ConfigureAwait(false);

        // The id if the message we used to subscribe should be used as the filter for the event messages
        var observableResult = _hassMessageSubject.Where(n => n.Type == "event" && n.Id == result?.Id)
            .Select(n => n.Event!);

        return observableResult;
    }

    public async Task WaitForConnectionToCloseAsync(CancellationToken cancelToken)
    {
        var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancelToken,
            _internalCancelSource.Token
        );

        // Just wait for token source (internal och provided one)
        await combinedTokenSource.Token.AsTask().ConfigureAwait(false);
    }

    public async Task SendCommandAsync<T>(T command, CancellationToken cancelToken) where T : CommandMessage
    {
        var returnMessageTask = await SendCommandAsyncInternal(command, cancelToken);
        _resultMessageHandler.HandleResult(returnMessageTask, command);
    }

    public async Task<TResult?> SendCommandAndReturnResponseAsync<T, TResult>(T command, CancellationToken cancelToken)
        where T : CommandMessage
    {
        var result = await SendCommandAndReturnResponseRawAsync(command, cancelToken).ConfigureAwait(false);

        return result is not null ? result.Value.Deserialize<TResult>() : default;
    }

    public async Task<JsonElement?> SendCommandAndReturnResponseRawAsync<T>(T command, CancellationToken cancelToken)
        where T : CommandMessage
    {
        var hassMessage =
            await SendCommandAndReturnHassMessageResponseAsync(command, cancelToken).ConfigureAwait(false);

        // The SendCommandsAndReturnHAssMessageResponse will throw if not successful so just ignore errors here
        return hassMessage?.ResultElement;
    }

    public async Task<HassMessage?> SendCommandAndReturnHassMessageResponseAsync<T>(T command,
        CancellationToken cancelToken)
        where T : CommandMessage
    {
        var resultMessageTask = await SendCommandAsyncInternal(command, cancelToken);
        var result = await resultMessageTask.WaitAsync(TimeSpan.FromMilliseconds(WaitForResultTimeout), cancelToken);

        if (result.Success ?? false)
            return result;

        // Non successful command should throw exception
        throw new InvalidOperationException(
            $"Failed command ({command.Type}) error: {result.Error}.  Sent command is {command.ToJsonElement()}");
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await CloseAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogDebug(e, "Failed to close HomeAssistantConnection");
        }

        if (!_internalCancelSource.IsCancellationRequested)
            await _internalCancelSource.CancelAsync();

        // Gracefully wait for task or timeout
        await Task.WhenAny(
            _handleNewMessagesTask,
            Task.Delay(5000)
        ).ConfigureAwait(false);

        await _transportPipeline.DisposeAsync().ConfigureAwait(false);
        _internalCancelSource.Dispose();
        _hassMessageSubject.Dispose();
        await _resultMessageHandler.DisposeAsync();
        _messageIdSemaphore.Dispose();
    }

    public Task<T?> GetApiCallAsync<T>(string apiPath, CancellationToken cancelToken)
    {
        return _apiManager.GetApiCallAsync<T>(apiPath, cancelToken);
    }

    public Task<T?> PostApiCallAsync<T>(string apiPath, CancellationToken cancelToken, object? data = null)
    {
        return _apiManager.PostApiCallAsync<T>(apiPath, cancelToken, data);
    }

    public IObservable<HassMessage> OnHassMessage => _hassMessageSubject;

    private async Task<Task<HassMessage>> SendCommandAsyncInternal<T>(T command, CancellationToken cancelToken) where T : CommandMessage
    {
        // The semaphore can fail to be taken in rare cases so we need
        // to keep this out of the try/finally block so it will not be released
        await _messageIdSemaphore.WaitAsync(cancelToken).ConfigureAwait(false);
        try
        {
            // We need to make sure messages to HA are send with increasing Ids therefore we need to synchronize
            // increasing the messageId and Sending the message
            command.Id = ++_messageId;

            // We make a task that subscribe for the return result message
            // this task will be returned and handled by caller
            var resultEvent = _hassMessageSubject
                .Where(n => n.Type == "result" && n.Id == command.Id)
                .FirstAsync().ToTask(CancellationToken.None);
            // We dont want to pass the incoming CancellationToken here because it will throw a TaskCanceledException
            // when calling services from an Apps Dispose(Async) and hide possible actual exceptions

            await _transportPipeline.SendMessageAsync(command, cancelToken);

            return resultEvent;
        }
        finally
        {
            _messageIdSemaphore.Release();
        }
    }

    private async Task HandleNewMessages()
    {
        try
        {
            while (!_internalCancelSource.IsCancellationRequested)
            {
                var msg = await _transportPipeline.GetNextMessagesAsync<HassMessage>(_internalCancelSource.Token)
                    .ConfigureAwait(false);
                try
                {
                    foreach (var obj in msg)
                    {
                        _hassMessageSubject.OnNext(obj);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed processing new message from Home Assistant");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal case just exit
        }
        finally
        {
            _logger.LogTrace("Stop processing new messages");
            // make sure we always cancel any blocking operations
            if (!_internalCancelSource.IsCancellationRequested)
                await _internalCancelSource.CancelAsync();
        }
    }

    private Task CloseAsync()
    {
        return _transportPipeline.CloseAsync();
    }
}
