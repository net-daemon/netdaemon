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
        _executingTask = _runtime.ExecuteAsync(cancellationToken);
        await _runtime.WhenStarted;
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_executingTask != null)
        {
            await _executingTask.ConfigureAwait(false);
        }
    }
}