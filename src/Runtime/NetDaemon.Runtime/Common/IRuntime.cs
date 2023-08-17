namespace NetDaemon.Runtime;

internal interface IRuntime : IAsyncDisposable
{
    Task StartAsync(CancellationToken stoppingToken);
}