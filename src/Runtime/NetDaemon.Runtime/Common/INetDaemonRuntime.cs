namespace NetDaemon.Runtime;

/// <summary>
/// The NetDaemon runtime interface.
/// </summary>
public interface INetDaemonRuntime : IAsyncDisposable
{
    /// <summary>
    /// Starts the runtime and passes <paramref name="stoppingToken"/> to the runtime task/thread.
    /// Will return a task that completes when the NetDaemon runtime is initialized.
    ///
    /// Calling this method multiple times will only start the runtime once, but can be useful for other processes if they want to await the NetDaemon runtime to be initialized.
    /// </summary>
    Task EnsureInitializedAsync(CancellationToken stoppingToken = default);

    /// <summary>
    /// Determines auto reconnect behavior of the runtime.
    /// </summary>
    AutoReconnectOptions AutoReconnectOptions { get; set; }
}
