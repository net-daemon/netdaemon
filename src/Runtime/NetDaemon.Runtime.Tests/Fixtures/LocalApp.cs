using NetDaemon.AppModel;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;

namespace LocalApps;

[NetDaemonApp]
public class LocalApp
{
    public LocalApp(
        IHaContext ha
    )
    {
        ha.StateChanges()
            .Where(n => n.Entity.EntityId == "binary_sensor.mypir" && n.New?.State == "on")
            .Subscribe(_ =>
            {
                ha.CallService("light", "turn_on", ServiceTarget.FromEntities("light.my_light"));
            });

        ha.StateChanges()
            .Where(n => n.Entity.EntityId == "binary_sensor.mypir_creates_fault" && n.New?.State == "on")
            .Subscribe(_ =>
            {
                throw new InvalidOperationException("Ohh nooo!");
            });
    }
}