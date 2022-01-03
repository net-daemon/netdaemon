using NetDaemon.AppModel;
using NetDaemon.HassModel.Common;

namespace AppModelApps;

[NetDaemonApp]
public class HelloWorldApp
{
    public HelloWorldApp(
        IHaContext ha
    )
    {
        ha.CallService("notify", "persistent_notification",
            data: new
            {
                message = "This is a message from appmodel and new client!",
                title = "Hello AppModel!"
            });
    }
}