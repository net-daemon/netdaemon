using NetDaemon.AppModel;
using NetDaemon.HassModel.Common;

namespace Apps;

[NetDaemonApp]
public class HelloApp
{
    public HelloApp(IHaContext ha)
    {
        ha?.CallService("notify", "persistent_notification", data: new { message = "Notify me", title = "Hello world!" });
    }
}