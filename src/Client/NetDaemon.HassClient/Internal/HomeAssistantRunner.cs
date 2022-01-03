namespace NetDaemon.Client.Internal;

internal class HomeAssistantRunner : IHomeAssistantRunner
{
    private readonly IHomeAssistantClient _client;

    // The internal token source will make sure we 
    // always cancel operations on dispose
    private readonly CancellationTokenSource _internalTokenSource = new();
    private Task? _runTask;
    public HomeAssistantRunner(
        IHomeAssistantClient client,
        ILogger<IHomeAssistantRunner> logger
    )
    {
        _client = client;
        _logger = logger;
    }
    private readonly Subject<IHomeAssistantConnection> _onConnectSubject = new();
    public IObservable<IHomeAssistantConnection> OnConnect => _onConnectSubject;

    private readonly Subject<DisconnectReason> _onDisconnectSubject = new();
    public IObservable<DisconnectReason> OnDisconnect => _onDisconnectSubject;

    private ILogger<IHomeAssistantRunner> _logger { get; }


    private IHomeAssistantConnection? _currentConnection;
    public IHomeAssistantConnection? CurrentConnection => _currentConnection;
    public Task RunAsync(string host, int port, bool ssl, string token, TimeSpan timeout, CancellationToken cancelToken)
    {
        _runTask = InternalRunAsync(host, port, ssl, token, timeout, cancelToken);
        return _runTask;
    }

    private async Task InternalRunAsync(string host, int port, bool ssl, string token, TimeSpan timeout, CancellationToken cancelToken)
    {
        var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(_internalTokenSource.Token, cancelToken);
        bool isRetry = false;
        while (!combinedToken.IsCancellationRequested)
        {
            if (isRetry)
            {
                _logger.LogDebug("Client disconnected, retrying in {seconds} seconds...", timeout.TotalSeconds);
                // This is a retry
                await Task.Delay(timeout, combinedToken.Token).ConfigureAwait(false);
            }
            try
            {
                _currentConnection = await _client.ConnectAsync(host, port, ssl, token, combinedToken.Token).ConfigureAwait(false);
                // Start the event processing before publish the connection
                var eventsTask = _currentConnection.ProcessHomeAssistantEventsAsync(combinedToken.Token);
                _onConnectSubject.OnNext(_currentConnection);
                await eventsTask.ConfigureAwait(false);
            }
            catch (HomeAssistantConnectionException de)
            {
                switch (de.Reason)
                {
                    case DisconnectReason.Unauthorized:
                        _logger.LogDebug("User token unauthorized! Will not retry connecting...");
                        _onDisconnectSubject.OnNext(DisconnectReason.Unauthorized);
                        return;
                    case DisconnectReason.NotReady:
                        _logger.LogDebug("Home Assistant is not ready yet!");
                        _onDisconnectSubject.OnNext(DisconnectReason.NotReady);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Run cancelled");
                if (_internalTokenSource.IsCancellationRequested)
                {
                    // We have internal cancellation due to dispose
                    // just return without any further due
                    return;
                }
                if (cancelToken.IsCancellationRequested)
                    _onDisconnectSubject.OnNext(DisconnectReason.Client);
                else
                    _onDisconnectSubject.OnNext(DisconnectReason.Remote);
            }
            catch (Exception e)
            {
                _logger.LogError("Error running HassClient", e);
                _onDisconnectSubject.OnNext(DisconnectReason.Error);
            }
            finally
            {
                if (_currentConnection is not null)
                {
                    // Just try to dispose the connection silently
                    try
                    {
                        await _currentConnection.DisposeAsync().ConfigureAwait(false);
                    }
                    finally
                    {
                        _currentConnection = null;
                    }
                }
            }
            isRetry = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        _internalTokenSource.Cancel();
        if (_runTask?.IsCompleted == false)
        {
            try
            {
                await Task.WhenAny(
                    _runTask,
                    Task.Delay(5000)
                ).ConfigureAwait(false);
            }
            catch
            {
                // Ignore errors
            }
        }
        _onConnectSubject.Dispose();
        _onDisconnectSubject.Dispose();
        _internalTokenSource.Dispose();
    }
}