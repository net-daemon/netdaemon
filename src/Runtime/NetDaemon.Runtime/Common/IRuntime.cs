namespace NetDaemon.Runtime;

public interface IRuntime : IAsyncDisposable
{
    Task StartAsync(CancellationToken stoppingToken);
}