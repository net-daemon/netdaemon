namespace NetDaemon.Runtime;

public interface IRuntime : IAsyncDisposable
{
    Task ExecuteAsync(CancellationToken stoppingToken);
    Task WhenStarted { get; }
}