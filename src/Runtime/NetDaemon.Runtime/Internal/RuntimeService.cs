namespace NetDaemon.Runtime.Internal;

internal class RuntimeService(IRuntime runtime, ILogger<RuntimeService> logger) : BackgroundService
{
    private Task? task;
    private CancellationTokenSource _stoppingTokenSource = new();

    public override Task StartAsync(CancellationToken _)
    {
        task = runtime.StartAsync(_stoppingTokenSource.Token);
        return Task.CompletedTask;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("NetDaemon RuntimeService is stopping");
        await _stoppingTokenSource.CancelAsync();
        await runtime.DisposeAsync();
        if (task is not null)
        {
            await task;
        }
        _stoppingTokenSource.Dispose();
    }

    public override void Dispose()
    {
        _stoppingTokenSource.Dispose();
        base.Dispose();
    }
}
