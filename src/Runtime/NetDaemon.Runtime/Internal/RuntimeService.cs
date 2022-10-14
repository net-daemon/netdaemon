namespace NetDaemon.Runtime.Internal;

internal class RuntimeService : BackgroundService
{
    private readonly IRuntime _runtime;
    
    private Task? _executingTask;

    public RuntimeService(IRuntime runtime)
    {
        _runtime = runtime;
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
}