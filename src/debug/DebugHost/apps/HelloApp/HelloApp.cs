using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace Apps;

[NetDaemonApp]
[Focus]
public sealed class HelloApp2 : IAsyncDisposable
{
    private readonly ILogger<HelloApp2> _logger;

    public HelloApp2(IHaContext ha, ILogger<HelloApp2> logger)
    {
        _logger = logger;
        var boilerConnected = ha.Entity("switch.heating_valve_kitchen");
        var labels = boilerConnected.Registration.Labels;
        var all = ha.GetAllEntities();
        var criticalEntities = ha.GetAllEntities().Where(e => e.Registration.Labels.Any(l => l.Name == "critical"));
        //criticalEntities.StateChanges().Where(s => s.New?.State == "unavailable").Subscribe(e => logger.LogCritical("Entity {Entity} became unavailable", e.Entity.EntityId));
    }

    public async ValueTask DisposeAsync()
    {
        await Task.Delay(5000);
        _logger.LogInformation("disposed app");
        //return ValueTask.CompletedTask;
    }
}
