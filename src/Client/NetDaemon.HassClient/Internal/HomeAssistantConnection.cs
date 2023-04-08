using System.Collections.Concurrent;
using NetDaemon.Client.Common.HomeAssistant.Model;

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
    private readonly ConcurrentDictionary<int, Subject<HassMessage>> _triggerSubscriptions = new();
    private readonly Task _handleNewMessagesTask;

    private const int WaitForResultTimeout = 20000;

    private readonly SemaphoreSlim _messageIdSemaphore = new(1,1);
    private int _messageId = 1;

    #endregion

    /// <summary>
    ///     Default constructor
    /// </summary>
    /// <param name="logger">A logger instance</param>
    /// <param name="pipeline">The pipeline to use for websocket communication</param>
    /// <param name="apiManager">The api manager</param>
    /// <param name="resultMessageHandler">Handler for result message</param>
    public HomeAssistantConnection(
        ILogger<IHomeAssistantConnection> logger,
        IWebSocketClientTransportPipeline pipeline,
        IHomeAssistantApiManager apiManager
    )
    {
        _transportPipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _apiManager = apiManager;
        _logger = logger;

        _resultMessageHandler = new(_logger);

        if (_transportPipeline.WebSocketState != WebSocketState.Open)
            throw new ApplicationException(
                $"Expected WebSocket state 'Open' got '{_transportPipeline.WebSocketState}'");

        _handleNewMessagesTask = Task.Factory.StartNew(async () => await HandleNewMessages().ConfigureAwait(false),
            TaskCreationOptions.LongRunning);
    }

    public IObservable<HassEvent> OnHomeAssistantEvent =>
        _hassMessageSubject.Where(n => n.Type == "event").Select(n => n.Event!);

    public async Task ProcessHomeAssistantEventsAsync(CancellationToken cancelToken)
    {
        var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancelToken,
            _internalCancelSource.Token
        );
        // Start by subscribing to all events
        await SubscribeToAllHomeAssistantEvents(combinedTokenSource.Token).ConfigureAwait(false);
        await combinedTokenSource.Token.AsTask().ConfigureAwait(false);
    }

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

        // The SendCommmandsAndReturnHAssMessageResponse will throw if not successful so just ignore errors here
        return hassMessage?.ResultElement;
    }
    
    public async Task<HassMessage?> SendCommandAndReturnHassMessageResponseAsync<T>(T command, CancellationToken cancelToken)
        where T : CommandMessage
    {
        var resultMessageTask = await SendCommandAsyncInternal(command, cancelToken);

        var awaitedTask = await Task.WhenAny(resultMessageTask, Task.Delay(WaitForResultTimeout, cancelToken));

        if (awaitedTask != resultMessageTask)
        {
            // We have a timeout
            throw new InvalidOperationException($"Send command ({command.Type}) did not get response in timely fashion. Sent command is {command.ToJsonElement()}");
        }

        if (resultMessageTask.Result.Success ?? false)
            return resultMessageTask.Result;

        // Non successful command should throw exception
        throw new InvalidOperationException($"Failed command ({command.Type}) error: {resultMessageTask.Result.Error}.  Sent command is {command.ToJsonElement()}");
    }

    public async Task<IObservable<HassMessage>> SubscribeToTriggerAsync<T>(
        T trigger, CancellationToken cancelToken) where T: TriggerBase
    {
        var triggerCommand = new SubscribeTriggersCommand<T>(trigger);
        
        var msg = await SendCommandAndReturnHassMessageResponseAsync<SubscribeTriggersCommand<T>>
                          (triggerCommand, cancelToken).ConfigureAwait(false) ??
                  throw new NullReferenceException("Unexpected null return from command");

        var triggerSubject = new Subject<HassMessage>();
        _triggerSubscriptions[msg.Id] = triggerSubject;
        
        return triggerSubject;
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
            _internalCancelSource.Cancel();

        // Gracefully wait for task or timeout
        await Task.WhenAny(
            _handleNewMessagesTask,
            Task.Delay(5000)
        ).ConfigureAwait(false);

        await _transportPipeline.DisposeAsync().ConfigureAwait(false);
        _internalCancelSource.Dispose();
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

    private async Task SubscribeToAllHomeAssistantEvents(CancellationToken cancelToken)
    {
        _ = await SendCommandAndReturnResponseAsync<SubscribeEventCommand, object?>(new SubscribeEventCommand(),
            cancelToken).ConfigureAwait(false);
    }

    private async Task HandleNewMessages()
    {
        try
        {
            while (!_internalCancelSource.IsCancellationRequested)
            {
                var msg = await _transportPipeline.GetNextMessageAsync<HassMessage>(_internalCancelSource.Token)
                    .ConfigureAwait(false);
                try
                {
                    if (_triggerSubscriptions.ContainsKey(msg.Id))
                    {
                        _triggerSubscriptions[msg.Id].OnNext(msg);                  
                    }
                    else
                    {
                        _hassMessageSubject.OnNext(msg);
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
                _internalCancelSource.Cancel();
        }
    }

    private Task CloseAsync()
    {
        return _transportPipeline.CloseAsync();
    }
}
