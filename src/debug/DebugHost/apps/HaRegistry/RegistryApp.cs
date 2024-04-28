using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
namespace Apps;

[NetDaemonApp]
[Focus]
public sealed class RegistryApp
{
    public RegistryApp(IHaRegistry haRegistry, IHaContext ha)
    {
        // var floor = haRegistry.GetFloor("upstairs");
        // var upstairsAreas = floor.Areas;
        // var upstairsBooleans = upstairsAreas
        //     .SelectMany(n => n.Entities
        //         .Where(x => x.EntityId.StartsWith("input_boolean.")));
        //
        // upstairsBooleans.ToList().ForEach(x => x.CallService("toggle"));
        ha.CallService("input_boolean", "toggle", new ServiceTarget{ FloorIds = ["upstairs"] });
    }
}
