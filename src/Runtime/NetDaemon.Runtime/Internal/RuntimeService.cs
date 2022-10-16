namespace NetDaemon.Runtime.Internal;

internal class RuntimeService : BackgroundService
{
    private readonly IRuntime _runtime;
    private readonly ILogger<RuntimeService> _logger;

    public RuntimeService(IRuntime runtime, ILogger<RuntimeService> logger)
    {
        _runtime = runtime;
        _logger = logger;
    }
    
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _runtime.StartAsync(cancellationToken);
            await base.StartAsync(cancellationToken);
        }
        catch (OperationCanceledException) { }
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("NetDaemon RuntimeService is stopping");
        await _runtime.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}