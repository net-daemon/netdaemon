using System.Linq;
using System;
using System.Reactive.Linq;
using NetDaemon.Common.Reactive;

namespace NetDaemon.Daemon.Tests.Reactive
{

    /// <summary> cool multiple lines </summary>
    public class FakeApp : NetDaemonRxApp
    {
        public override void Initialize()
        {
            Entity("binary_sensor.kitchen")
                .StateChanges
                .Where(e => e.New?.State == "on" && e.Old?.State == "off")
                .Subscribe(s =>
                {
                    Entity("light.kitchen").TurnOn();
                });
        }
    }
}
