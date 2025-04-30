namespace NetDaemon.Runtime.Internal;

internal class RuntimeService(IRuntime runtime, ILogger<RuntimeService> logger) : BackgroundService
{
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            runtime.Start(cancellationToken);
            await base.StartAsync(cancellationToken);
        }
        catch (OperationCanceledException) { }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("NetDaemon RuntimeService is stopping");
        await runtime.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
