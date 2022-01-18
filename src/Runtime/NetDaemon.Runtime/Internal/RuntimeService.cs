namespace NetDaemon.Runtime.Internal;

internal class RuntimeService : BackgroundService
{
    private readonly IHostApplicationLifetime _hostLifetime;

    private readonly IRuntime _runtime;

    public RuntimeService(
        IHostApplicationLifetime hostLifetime,
        IRuntime runtime
        )
    {
        _hostLifetime = hostLifetime;
        _runtime = runtime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _runtime.ExecuteAsync(stoppingToken).ConfigureAwait(false);
        // Stop application if this is exited
        _hostLifetime.StopApplication();
    }
}