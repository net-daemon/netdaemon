namespace NetDaemon.Runtime;

internal interface IRuntime : IAsyncDisposable
{
    /// <summary>
    /// Starts the runtime and passes <paramref name="stoppingToken"/> to the runtime task/thread.
    /// </summary>
    void Start(CancellationToken stoppingToken);
}
