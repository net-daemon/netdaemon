using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using Xunit;
using System.Threading;
using Moq;
using System;

namespace NetDaemon.Daemon.Tests
{
    /// <summary>
    ///     Tests the fluent API parts of the daemon
    /// </summary>
    /// <remarks>
    ///     Mainly the tests checks if correct underlying call to "CallService"
    ///     has been made. 
    /// </remarks>
    public class FluentTests
    {
        [Fact]
        public async Task TurnOffEntityCallsCorrectServiceCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            await daemonHost
                .Entity("light.correct_entity")
                    .TurnOff()
                .ExecuteAsync();

            // ASSERT
            hcMock.VerifyCallServiceTimes("turn_off", Times.Once());
            hcMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task TurnOffMultipleEntityCallsCorrectServiceCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            await daemonHost
                .Entity("light.correct_entity", "light.correct_entity2")
                    .TurnOff()
                .ExecuteAsync();

            // ASSERT
            hcMock.VerifyCallServiceTimes("turn_off", Times.Exactly(2));

            hcMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity"));
            hcMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity2"));

        }

        [Fact]
        public async Task TurnOffEntityLamdaSelectionCallsCorrectServiceCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            await daemonHost
                .Entities(n => n.EntityId.StartsWith("light.correct_entity"))
                    .TurnOff()
                .ExecuteAsync();

            // ASSERT

            hcMock.VerifyCallServiceTimes("turn_off", Times.Exactly(2));
            hcMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity"));
            hcMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity2"));

        }
        [Fact]
        public async Task TurnOffEntityLambdaAttributeSelectionCallsCorrectServiceCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            await daemonHost
                .Entities(n => n.Attribute.test >= 100)
                    .TurnOff()
                .ExecuteAsync();

            // ASSERT

            hcMock.VerifyCallServiceTimes("turn_off", Times.Exactly(3));
            hcMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity"));
            hcMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity2"));
            hcMock.VerifyCallService("switch", "turn_off", ("entity_id", "switch.correct_entity"));

        }

        [Fact]
        public async Task TurnOffEntityLambdaAttributeSelectionNoExistCallsCorrectServiceCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            await daemonHost
                .Entities(n => n.Attribute.not_exists == "test")
                    .TurnOff()
                .ExecuteAsync();

            // ASSERT

            hcMock.VerifyCallServiceTimes("turn_off", Times.Never());

        }


        [Fact]
        public async Task TurnOnEntityCallsCorrectServiceCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            await daemonHost
                .Entity("light.correct_entity")
                    .TurnOn()
                .ExecuteAsync();

            // ASSERT
            hcMock.VerifyCallServiceTimes("turn_on", Times.Once());
            hcMock.VerifyCallService("light", "turn_on", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task TurnOnEntityWithAttributeCallsCorrectServiceCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            await daemonHost
                .Entity("light.correct_entity")
                    .TurnOn()
                        .UsingAttribute("brightness", 100)
                .ExecuteAsync();

            // ASSERT
            hcMock.VerifyCallServiceTimes("turn_on", Times.Once());
            hcMock.VerifyCallService("light", "turn_on",
                ("brightness", 100),
                ("entity_id", "light.correct_entity"));

        }


        [Fact]
        public async Task TurnOnEntityWithMultipleAttributeCallsCorrectServiceCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            await daemonHost
                .Entity("light.correct_entity")
                    .TurnOn()
                        .UsingAttribute("brightness", 100)
                        .UsingAttribute("color_temp", 230)
                .ExecuteAsync();

            // ASSERT
            hcMock.VerifyCallServiceTimes("turn_on", Times.Once());
            hcMock.VerifyCallService("light", "turn_on",
                ("brightness", 100),
                ("color_temp", 230),
                ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task ToggleEntityCallsCorrectServiceCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            await daemonHost
                .Entity("light.correct_entity")
                    .Toggle()
                .ExecuteAsync();

            // ASSERT
            hcMock.VerifyCallServiceTimes("toggle", Times.Once());
            hcMock.VerifyCallService("light", "toggle", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task TurnOffLightWithoutDomainCallsCorrectServiceCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            await daemonHost
                .Light("correct_entity")
                    .TurnOff()
                .ExecuteAsync();

            // ASSERT
            hcMock.VerifyCallServiceTimes("turn_off", Times.Once());
            hcMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task TurnOffLightLambdaAttributeSelectionCallsCorrectServiceCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            await daemonHost
                .Lights(n => n.Attribute.test >= 100)
                    .TurnOff()
                .ExecuteAsync();

            // ASSERT
            hcMock.VerifyCallServiceTimes("turn_off", Times.Exactly(2));
            hcMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity"));
            hcMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity2"));
        }

        [Fact]
        public async Task EntityOnStateChangedTurnOnLightCallsCorrectServiceCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            hcMock.AddChangedEvent("binary_sensor.pir", fromState:"off", toState: "on");
 
            CancellationTokenSource cancelSource = hcMock.GetSourceWithTimeout(10);

            daemonHost
                .Entity("binary_sensor.pir")
                    .StateChanged(to: "on")
                        .Entity("light.correct_entity")
                            .TurnOn()
                .Execute();

            try
            {
                await daemonHost.Run("host", 8123, false, "token", cancelSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Expected behaviour
            }

            hcMock.VerifyCallServiceTimes("turn_on", Times.Once());
            hcMock.VerifyCallService("light", "turn_on", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task EntityOnStateChangedForTimeTurnOffLightCallsCorrectServiceCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            var lastChanged = new DateTime(2020, 1, 1, 1, 1, 1, 10);
            var lastUpdated = new DateTime(2020, 1, 1, 1, 1, 1, 50);


            hcMock.AddChangedEvent("binary_sensor.pir", fromState: "on", toState: "off",
               lastUpdated, lastChanged);

            CancellationTokenSource cancelSource = hcMock.GetSourceWithTimeout(10);

            daemonHost
                .Entity("binary_sensor.pir")
                    .StateChanged(to: "off")
                        .For(TimeSpan.FromMilliseconds(30))
                    .Entity("light.correct_entity")
                        .TurnOff()
                .Execute();

            try
            {
                await daemonHost.Run("host", 8123, false, "token", cancelSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Expected behaviour
            }

            // ASSERT
            await Task.Delay(10); // After 10ms we should not have call
            hcMock.VerifyCallServiceTimes("turn_on", Times.Never());

            await Task.Delay(30); // After 40ms we should have call
            hcMock.VerifyCallServiceTimes("turn_off", Times.Once());

        }

        [Fact]
        public async Task EntityOnStateChangedTurnOnLightWithAttributesCallsCorrectServiceCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            hcMock.AddChangedEvent("binary_sensor.pir", fromState: "off", toState: "on");

            var cancelSource = hcMock.GetSourceWithTimeout(10);

            daemonHost
                .Entity("binary_sensor.pir")
                    .StateChanged(to: "on")
                        .Entity("light.correct_entity")
                            .TurnOn()
                                .UsingAttribute("transition", 0)
                .Execute();

            try
            {
                await daemonHost.Run("host", 8123, false, "token", cancelSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Expected behaviour
            }

            hcMock.VerifyCallServiceTimes("turn_on", Times.Once());
            hcMock.VerifyCallService("light", "turn_on", 
                ("transition", 0),
                ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task EntityOnStateChangedMultipleTimesCallsCorrectServiceCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            hcMock.AddChangedEvent("binary_sensor.pir", fromState: "off", toState: "on");
            hcMock.AddChangedEvent("binary_sensor.pir", fromState: "off", toState: "on");

            var cancelSource = hcMock.GetSourceWithTimeout(10);

            daemonHost
                .Entity("binary_sensor.pir")
                   .StateChanged(to: "on")
                        .Entity("light.correct_entity")
                            .TurnOn()
                .Execute();

            try
            {
                await daemonHost.Run("host", 8123, false, "token", cancelSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Expected behaviour
            }

            hcMock.VerifyCallServiceTimes("turn_on", Times.Exactly(2));
            hcMock.VerifyCallService("light", "turn_on",
                ("entity_id", "light.correct_entity"));
        }


        [Fact]
        public async Task EntityOnStateChangedTurnOnLightCallsCorrectServiceCallButNoTurnOff()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            hcMock.AddChangedEvent("binary_sensor.pir", fromState: "off", toState: "on");

            
            var cancelSource = hcMock.GetSourceWithTimeout(10);
 
            daemonHost
                .Entity("binary_sensor.pir")
                    .StateChanged(to: "on")
                        .Entity("light.correct_entity")
                            .TurnOn()
                .Execute();

            daemonHost
                .Entity("binary_sensor.pir")
                    .StateChanged(to: "off")
                        .Entity("light.correct_entity")
                            .TurnOff()
                .Execute();
            
            try
            {
                await daemonHost.Run("host", 8123, false, "token", cancelSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Expected behaviour
            }

            hcMock.VerifyCallServiceTimes("turn_on", Times.Once());
            hcMock.VerifyCallService("light", "turn_on", ("entity_id", "light.correct_entity"));

            hcMock.VerifyCallServiceTimes("turn_off", Times.Never());
        
        }
        [Fact]
        public async Task EntityOnStateChangedLamdaTurnOnLightCallsCorrectServiceCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            hcMock.AddChangedEvent("binary_sensor.pir", fromState: "off", toState: "on");

            var cancelSource = hcMock.GetSourceWithTimeout(20);

            daemonHost
                .Entity("binary_sensor.pir")
                    .StateChanged((n,_) =>n.State=="on")
                        .Entity("light.correct_entity")
                            .TurnOn()
                    .Execute();

            try
            {
                await daemonHost.Run("host", 8123, false, "token", cancelSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Expected behaviour
            }

            hcMock.VerifyCallServiceTimes("turn_on", Times.Once());
            hcMock.VerifyCallService("light", "turn_on", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task TimerEveryShouldCallServiceCorrectNumberOfTimes()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            daemonHost
                .Timer()
                    .Every(TimeSpan.FromMilliseconds(20))
                        .Entity("light.correct_light")
                            .TurnOn()
                .Execute();

            var cancelSource = new CancellationTokenSource(100); //hcMock.GetSourceWithTimeout(100);

            // ACTION
            try
            {
                await daemonHost.Run("host", 8123, false, "token", cancelSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Expected behaviour
            }

            // ASSERT
            hcMock.VerifyCallServiceTimes("turn_on", Times.AtLeast(4));
            hcMock.VerifyCallService("light", "turn_on", ("entity_id", "light.correct_light"));

        }


    }



}
