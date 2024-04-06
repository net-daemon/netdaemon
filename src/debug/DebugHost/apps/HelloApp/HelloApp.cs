using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.HassModel;

namespace Apps;

[NetDaemonApp]
[Focus]
public sealed class HelloApp2 : IAsyncDisposable
{
    private readonly ILogger<HelloApp2> _logger;

    public HelloApp2(IHaContext ha, ILogger<HelloApp2> logger)
    {
        _logger = logger;
        var boilerConnected = ha.Entity("binary_sensor.opentherm_gateway_otgw_otgw_boiler_connected");
        var labels = boilerConnected.Registration.Labels;

        var criticalEntities = ha.GetAllEntities().Where(e => e.Registration.Labels.Any(l => l.Name == "critical"));
    }

    public async ValueTask DisposeAsync()
    {
        await Task.Delay(5000);
        _logger.LogInformation("disposed app");
        //return ValueTask.CompletedTask;
    }
}
