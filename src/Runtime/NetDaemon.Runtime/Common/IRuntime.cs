namespace NetDaemon.Runtime;

public interface IRuntime
{
    Task ExecuteAsync(CancellationToken stoppingToken);
}