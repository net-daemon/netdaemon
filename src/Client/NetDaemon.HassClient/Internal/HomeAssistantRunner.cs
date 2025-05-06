using NetDaemon.Client.Exceptions;

namespace NetDaemon.Client.Internal;

internal class HomeAssistantRunner(IHomeAssistantClient client,
    ILogger<IHomeAssistantRunner> logger) : IHomeAssistantRunner
{
    // The internal token source will make sure we
    // always cancel operations on dispose
    private readonly CancellationTokenSource _internalTokenSource = new();
    private readonly Subject<IHomeAssistantConnection> _onConnectSubject = new();

    private readonly Subject<DisconnectReason> _onDisconnectSubject = new();

    private readonly TimeSpan _maxTimeoutInSeconds = TimeSpan.FromSeconds(80);

    private Task? _runTask;

    public IObservable<IHomeAssistantConnection> OnConnect => _onConnectSubject;
    public IObservable<DisconnectReason> OnDisconnect => _onDisconnectSubject;
    public IHomeAssistantConnection? CurrentConnection { get; internal set; }

    public Task RunAsync(string host, int port, bool ssl, string token, TimeSpan timeout,
        CancellationToken cancelToken)
    {
        return RunAsync(host, port, ssl, token, HomeAssistantSettings.DefaultWebSocketPath, timeout, cancelToken);
    }
    public Task RunAsync(string host, int port, bool ssl, string token, string websocketPath, TimeSpan timeout, CancellationToken cancelToken)
    {
        _runTask = InternalRunAsync(host, port, ssl, token, websocketPath, timeout, cancelToken);
        return _runTask;
    }

    private bool _isDisposed;
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;
        await _internalTokenSource.CancelAsync();

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
        _isDisposed = true;
    }

    private async Task InternalRunAsync(string host, int port, bool ssl, string token, string websocketPath, TimeSpan timeout,
        CancellationToken cancelToken)
    {
        var progressiveTimeout = new ProgressiveTimeout(timeout, _maxTimeoutInSeconds, 2.0);
        var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(_internalTokenSource.Token, cancelToken);
        while (!combinedToken.IsCancellationRequested)
        {
            try
            {
                CurrentConnection = await client.ConnectAsync(host, port, ssl, token, websocketPath, combinedToken.Token)
                    .ConfigureAwait(false);
                // We successfully connected so lets reset the progressiveTimeout
                progressiveTimeout.Reset();

                // Start the event processing before publish the connection
                var eventsTask = CurrentConnection.WaitForConnectionToCloseAsync(combinedToken.Token);
                _onConnectSubject.OnNext(CurrentConnection);
                await eventsTask.ConfigureAwait(false);
            }
            catch (HomeAssistantConnectionException de) when (de.Reason == DisconnectReason.Unauthorized)
            {
                logger.LogError("User token unauthorized! Will not retry connecting...");
                await DisposeConnectionAsync();
                _onDisconnectSubject.OnNext(DisconnectReason.Unauthorized);
                return;
            }
            catch (HomeAssistantConnectionException de) when (de.Reason == DisconnectReason.NotReady)
            {
                logger.LogInformation("Home Assistant is not ready yet!");
                await DisposeConnectionAsync();
                // We have an connection but waiting for Home Assistant to be ready so lets reset the progressiveTimeout
                progressiveTimeout.Reset();
                _onDisconnectSubject.OnNext(DisconnectReason.NotReady);
            }
            catch (OperationCanceledException)
            {
                await DisposeConnectionAsync();
                if (_internalTokenSource.IsCancellationRequested)
                {
                    // We have internal cancellation due to dispose
                    // just return without any further due
                    return;
                }

                _onDisconnectSubject.OnNext(cancelToken.IsCancellationRequested
                    ? DisconnectReason.Client
                    : DisconnectReason.Remote);
            }
            catch (Exception e)
            {
                // In most cases this is just normal when client fails to connect so we log as debug
                logger.LogDebug(e, "Unhandled exception connecting to Home Assistant!");
                await DisposeConnectionAsync();
                _onDisconnectSubject.OnNext(DisconnectReason.Error);
            }

            await DisposeConnectionAsync();

            if (combinedToken.IsCancellationRequested)
                return; // If we are cancelled we should not retry

            var waitTimeout = progressiveTimeout.Timeout;
            logger.LogInformation("Client connection failed, retrying in {Seconds} seconds...", waitTimeout.TotalSeconds);
            await Task.Delay(waitTimeout, combinedToken.Token).ConfigureAwait(false);
        }
    }

    private async Task DisposeConnectionAsync()
    {
        if (CurrentConnection is not null)
        {
            // Just try to dispose the connection silently
            try
            {
                await CurrentConnection.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                CurrentConnection = null;
            }
        }
    }
}
