namespace NetDaemon.Runtime;

/// <summary>
/// The NetDaemon runtime interface.
/// </summary>
public interface INetDaemonRuntime : IAsyncDisposable
{
    /// <summary>
    /// Enum to determine the auto reconnect behavior of the runtime.
    /// Default is StopReconnectOnUnAuthorized.
    /// </summary>
    AutoReconnectOptions AutoReconnectOptions { get; set; }

    /// <summary>
    /// Method that can be awaited to ensure that the runtime is fully started and initialized (initial connection is created and cache is initialized).
    /// </summary>
    Task WaitForInitializationAsync();
}
