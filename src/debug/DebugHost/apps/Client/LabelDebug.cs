
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.Client;
using NetDaemon.HassModel;
using NetDaemon.Client.HomeAssistant.Extensions;

namespace Apps;

[NetDaemonApp]
[Focus]
public sealed class LabelApp : IAsyncInitializable
{
    private readonly ILogger<HelloApp> _logger;
    private readonly IHomeAssistantConnection _conn;

    public LabelApp(ILogger<HelloApp> logger, IHomeAssistantConnection conn)
    {
        _logger = logger;
        _conn = conn;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting labels");
        var labels = await _conn.GetLabelsAsync(cancellationToken).ConfigureAwait(false);
        if (labels == null)
        {
            _logger.LogInformation("No labels found");
            return;
        }
        foreach (var label in labels)
        {
            _logger.LogInformation("Label: {Label}", label);
        }

        _logger.LogInformation("Getting floors");
        var floors = await _conn.GetFloorsAsync(cancellationToken).ConfigureAwait(false);
        if (floors == null)
        {
            _logger.LogInformation("No floors found");
            return;
        }
        foreach (var floor in floors)
        {
            _logger.LogInformation("Floor: {Floor}", floor);
        }
    }
}

