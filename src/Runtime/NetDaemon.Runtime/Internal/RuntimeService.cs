namespace NetDaemon.Runtime.Internal;

internal class RuntimeService(INetDaemonRuntime netDaemonRuntime, ILogger<RuntimeService> logger) : BackgroundService
{
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Note: we purposely do not await this method as we don't want to block the startup of the host. EnsureInitializedAsync can be called again to get access to the same Task if another process wants to await it.
            _ = netDaemonRuntime.EnsureInitializedAsync(cancellationToken);
            await base.StartAsync(cancellationToken);
        }
        catch (OperationCanceledException) { }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("NetDaemon RuntimeService is stopping");
        await netDaemonRuntime.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
