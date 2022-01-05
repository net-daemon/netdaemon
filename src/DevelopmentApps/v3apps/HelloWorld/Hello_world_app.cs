using System;
using NetDaemon.AppModel;
using NetDaemon.HassModel.Common;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace AppModelApps;

[NetDaemonApp]
public class HelloWorldApp
{
    public HelloWorldApp(
        IHaContext ha,
        ILogger<HelloWorldApp> logger
    )
    {
        ha.StateAllChanges().Subscribe(_ => logger.LogInformation("New event"));
        // ha.CallService("notify", "persistent_notification",
        //     data: new
        //     {
        //         message = "This is a message from appmodel and new client!",
        //         title = "Hello AppModel!"
        //     });
    }
}

[NetDaemonApp]
public class HelloWithException
{
    public HelloWithException(
        IHaContext ha,
        ILogger<HelloWithException> logger
    )
    {
        //Where(n => n.New?.EntityId.Length > 0
        ha.StateAllChanges().Subscribe(
            _ =>
            {
                throw new InvalidOperationException("What the hell!");
            }
        );
        ha.StateAllChanges().Subscribe(
            _ =>
            {
                Thread.Sleep(1000);
                logger.LogInformation("New event from error app 1s delay");
            }
        );
        ha.StateAllChanges().Subscribe(
            _ =>
            {
                Thread.Sleep(2000);
                logger.LogInformation("New event from error app 2s delay");
            }
        );
    }
}