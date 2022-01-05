using System.Reactive.Linq;
using NetDaemon.AppModel;
using NetDaemon.HassModel;

namespace NetDaemon.Runtime.Internal;

internal class RuntimeService : BackgroundService
{
    private readonly IHostApplicationLifetime _hostLifetime;

    private readonly ILogger<RuntimeService> _logger;
    private readonly IRuntime _runtime;

    public RuntimeService(
        IHostApplicationLifetime hostLifetime,
        IRuntime runtime,
        ILogger<RuntimeService> logger)
    {
        _hostLifetime = hostLifetime;
        _logger = logger;
        _runtime = runtime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _runtime.ExecuteAsync(stoppingToken).ConfigureAwait(false);
        // Stop application if this is exited
        _hostLifetime.StopApplication();
    }
}