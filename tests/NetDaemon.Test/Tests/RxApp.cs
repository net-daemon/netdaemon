
using System;
using NetDaemon.Common.Reactive;
using System.Reactive.Linq;
namespace NetDaemon.Daemon.Test.Tests
{

    public class RxApp : NetDaemonRxApp
    {
        public override void Initialize()
        {
            Entity("binary_sensor.pir")
                .StateChanges
                .Where(e => e.New?.State == "on")
                .Subscribe(s =>
                {
                    Entity("light.thelight").TurnOn();
                }
                );
        }
    }
}