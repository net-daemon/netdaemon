using System.Linq;
using System;
using System.Reactive.Linq;
using NetDaemon.Common.Reactive;

namespace NetDaemon.Daemon.Tests.Reactive
{
    /// <summary> cool multiple lines </summary>
    public class FakeMockableApp : NetDaemonRxApp
    {
        private readonly FakeMockableAppImplementation _app;
        protected FakeMockableApp()
        {
            _app = new(this);
        }

        public override void Initialize()
        {
            _app.Initialize();
        }
    }

    public class FakeMockableAppImplementation
    {
        private readonly INetDaemonRxApp _app;
        private IDisposable? _timer;

        public FakeMockableAppImplementation(INetDaemonRxApp app)
        {
            _app = app;
        }

        /// <summary>
        ///     Initialize the app
        /// </summary>
        public void Initialize()
        {
            _app.Entity("binary_sensor.kitchen")
            .StateChanges
            .Where(e => e.New?.State == "on" && e.Old?.State == "off")
            .Subscribe(_ =>
                {
                    // Just for the testing of all these
                    _app.Entity("light.kitchen").TurnOn(new { brightness = 100 });
                    _app.Entity("light.kitchen").TurnOff(new { brightness = 100 });
                    _app.Entity("light.kitchen").Toggle(new { brightness = 100 });
                }
            );

            _app.Entity("binary_sensor.livingroom")
            .StateChanges
            .Where(e => e.New?.State == "on" && e.Old?.State == "off")
            .Subscribe(_ => _app.Entity("sensor.mysensor").SetState(20, new { battery_level = 90 }));

            _app.Entities(n => n.EntityId!.EndsWith("entities", StringComparison.InvariantCultureIgnoreCase)).StateChanges.Where(s => s.New.State == "on").Subscribe(_ => _app.Entity("light.kitchen").TurnOn());
            _app.Entities(n => n.EntityId!.EndsWith("entities", StringComparison.InvariantCultureIgnoreCase)).StateChanges.Where(s => s.New.State == "off").Subscribe(_ => _app.Entity("light.kitchen").TurnOff());

            _app.Entity("sensor.temperature")
                .StateAllChanges
                .Where(e => e.New?.Attribute?.battery_level < 15)
                .Subscribe(_ => _app.CallService("notify", "notify", new { title = "Hello from Home Assistant" }));

            _app.EventChanges
                .Where(e => e.Event == "hello_event")
                .Subscribe(_ => _app.Entity("light.livingroom").TurnOn());

            _app.EventChanges
                .Where(e => e.Event == "bye_event")
                .Subscribe(_ => _app.Entity("light.livingroom").TurnOff());

            _app.SetState("sensor.any_sensor", 20, new { battery_level = 70 });
            _app.SetState("sensor.any_sensor2", 20, new { battery_level = 70 });

            _app.Entity("binary_sensor.test_state_entity")
                .StateChanges
                .Subscribe(_ =>
                {
                    if (_app.State("sensor.some_other_entity")?.State == "on")
                        _app.Entity("light.state_light").TurnOn();
                });

            _app.RunIn(TimeSpan.FromMilliseconds(100), () => _app.Entity("binary_sensor.fake_run_in_happened").TurnOn());
            _app.RunEveryMinute(0, () => _app.Entity("binary_sensor.fake_run_every_minute_happened").TurnOn());
            // _app.RunEveryHour("15:00", () => _app.Entity("binary_sensor.fake_run_every_hour_happened").TurnOn());
            // _app.RunDaily("23:00:00", () => _app.Entity("binary_sensor.fake_run_daily_happened").TurnOn());
            // _timer = _app.RunEveryMinute(0, TestTimerDisposal);
        }

        private void TestTimerDisposal()
        {
            _app.Entity("binary_sensor.fake_run_stops_when_disposed").TurnOn();
            _timer!.Dispose();
        }
    }
}
