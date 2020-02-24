using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NetDaemon.Daemon.Tests
{
    /// <summary>
    ///     Tests the fluent API parts of the daemon
    /// </summary>
    /// <remarks>
    ///     Mainly the tests checks if correct underlying call to "CallService"
    ///     has been made.
    /// </remarks>
    public class FluentTests : DaemonHostTestaBase
    {
        public FluentTests() : base()
        {
        }

        [Fact]
        public async Task EntityOnStateChangedForTimeTurnOffLightCallsCorrectServiceCall()
        {
            // ARRANGE

            var lastChanged = new DateTime(2020, 1, 1, 1, 1, 1, 10);
            var lastUpdated = new DateTime(2020, 1, 1, 1, 1, 1, 50);

            var cancelSource = DefaultHassClientMock.GetSourceWithTimeout(100);

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "on", "off",
                lastUpdated, lastChanged);

            DefaultDaemonHost
                .Entity("binary_sensor.pir")
                .WhenStateChange("off")
                .AndNotChangeFor(TimeSpan.FromMilliseconds(20))
                .UseEntity("light.correct_entity")
                .TurnOff()
                .Execute();

            var (daemonTask, _) = ReturnRunningDefauldDaemonHostTask();
            // ASSERT
            await Task.Delay(10); // After 10ms we should not have call
            DefaultHassClientMock.VerifyCallServiceTimes("turn_off", Times.Never());
            await Task.Delay(30); // After 10ms we should not have call

            DefaultHassClientMock.VerifyCallServiceTimes("turn_off", Times.Once());

            await WaitUntilCanceled(daemonTask);
        }

        [Fact]
        public async Task EntityOnStateChangedLamdaTurnOnLightCallsCorrectServiceCall()
        {
            // ARRANGE

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            var cancelSource = DefaultHassClientMock.GetSourceWithTimeout();

            DefaultDaemonHost
                .Entity("binary_sensor.pir")
                .WhenStateChange((n, _) => n?.State == "on")
                .UseEntity("light.correct_entity")
                .TurnOn()
                .Execute();

            await RunDefauldDaemonUntilCanceled();

            DefaultHassClientMock.VerifyCallServiceTimes("turn_on", Times.Once());
            DefaultHassClientMock.VerifyCallService("light", "turn_on", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task EntityOnStateChangedLamdaToggleLightCallsCorrectServiceCall()
        {
            // ARRANGE
            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            var cancelSource = DefaultHassClientMock.GetSourceWithTimeout();

            DefaultDaemonHost
                .Entity("binary_sensor.pir")
                .WhenStateChange((n, _) => n?.State == "on")
                .UseEntity("light.correct_entity")
                .Toggle()
                .Execute();

            await RunDefauldDaemonUntilCanceled();

            DefaultHassClientMock.VerifyCallServiceTimes("toggle", Times.Once());
            DefaultHassClientMock.VerifyCallService("light", "toggle", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task EntityOnStateChangedMultipleTimesCallsCorrectServiceCall()
        {
            // ARRANGE
            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");
            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            var cancelSource = DefaultHassClientMock.GetSourceWithTimeout();

            DefaultDaemonHost
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .UseEntity("light.correct_entity")
                .TurnOn()
                .Execute();

            await RunDefauldDaemonUntilCanceled();

            DefaultHassClientMock.VerifyCallServiceTimes("turn_on", Times.Exactly(2));
            DefaultHassClientMock.VerifyCallService("light", "turn_on",
                ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task EntityOnStateChangedTurnOnLightCallsCorrectServiceCall()
        {
            // ARRANGE
            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            var cancelSource = DefaultHassClientMock.GetSourceWithTimeout();

            DefaultDaemonHost
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .UseEntity("light.correct_entity")
                .TurnOn()
                .Execute();

            await RunDefauldDaemonUntilCanceled();

            DefaultHassClientMock.VerifyCallServiceTimes("turn_on", Times.Once());
            DefaultHassClientMock.VerifyCallService("light", "turn_on", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task EntityOnStateChangedEntitiesTurnOnLightCallsCorrectServiceCall()
        {
            // ARRANGE
            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            var cancelSource = DefaultHassClientMock.GetSourceWithTimeout();

            DefaultDaemonHost
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .UseEntities(new string[] { "light.correct_entity" })
                .TurnOn()
                .Execute();

            await RunDefauldDaemonUntilCanceled();

            DefaultHassClientMock.VerifyCallServiceTimes("turn_on", Times.Once());
            DefaultHassClientMock.VerifyCallService("light", "turn_on", ("entity_id", "light.correct_entity"));
        }


        [Fact]
        public async Task EntityOnStateChangedEntitiesLambdaTurnOnLightCallsCorrectServiceCall()
        {
            // ARRANGE
            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            var cancelSource = DefaultHassClientMock.GetSourceWithTimeout(); //new CancellationTokenSource(1000);

            // Fake the
            DefaultDaemonHost.InternalState.Add("light.correct_entity", new EntityState
            {
                EntityId = "light.correct_entity"
            });

            DefaultDaemonHost
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .UseEntities(n => n.EntityId == "light.correct_entity")
                .TurnOn()
                .Execute();

            await RunDefauldDaemonUntilCanceled();

            DefaultHassClientMock.VerifyCallServiceTimes("turn_on", Times.Once());
            DefaultHassClientMock.VerifyCallService("light", "turn_on", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task EntityOnStateChangedTurnOnLightCallsCorrectServiceCallButNoTurnOff()
        {
            // ARRANGE
            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            var cancelSource = DefaultHassClientMock.GetSourceWithTimeout();

            DefaultDaemonHost
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .UseEntity("light.correct_entity")
                .TurnOn()
                .Execute();

            DefaultDaemonHost
                .Entity("binary_sensor.pir")
                .WhenStateChange("off")
                .UseEntity("light.correct_entity")
                .TurnOff()
                .Execute();

            await RunDefauldDaemonUntilCanceled();

            DefaultHassClientMock.VerifyCallServiceTimes("turn_on", Times.Once());
            DefaultHassClientMock.VerifyCallService("light", "turn_on", ("entity_id", "light.correct_entity"));

            DefaultHassClientMock.VerifyCallServiceTimes("turn_off", Times.Never());
        }

        [Fact]
        public async Task EntityOnStateChangedTurnOnLightWithAttributesCallsCorrectServiceCall()
        {
            // ARRANGE
            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            var cancelSource = DefaultHassClientMock.GetSourceWithTimeout();

            DefaultDaemonHost
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .UseEntity("light.correct_entity")
                .TurnOn()
                .WithAttribute("transition", 0)
                .Execute();

            await RunDefauldDaemonUntilCanceled();

            DefaultHassClientMock.VerifyCallServiceTimes("turn_on", Times.Once());
            DefaultHassClientMock.VerifyCallService("light", "turn_on",
                ("transition", 0),
                ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task EntityOnStateDefaultTriggerOnAnyStateChange()
        {
            // ARRANGE
            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            var cancelSource = DefaultHassClientMock.GetSourceWithTimeout();
            var triggered = false;

            // ACT
            DefaultDaemonHost
                .Entity("binary_sensor.pir")
                .WhenStateChange()
                .Call((e, n, o) =>
                {
                    triggered = true;
                    return Task.CompletedTask;
                })
                .Execute();

            await RunDefauldDaemonUntilCanceled();

            // ASSERT
            Assert.True(triggered);
        }

        [Fact]
        public async Task EntityOnStateNotTriggerOnSameState()
        {
            // ARRANGE
            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "off");

            var cancelSource = DefaultHassClientMock.GetSourceWithTimeout();
            var triggered = false;

            // ACT
            DefaultDaemonHost
                .Entity("binary_sensor.pir")
                .WhenStateChange()
                .Call((e, n, o) =>
                {
                    triggered = true;
                    return Task.CompletedTask;
                })
                .Execute();

            await RunDefauldDaemonUntilCanceled();

            // ASSERT
            Assert.False(triggered);
        }

        [Fact]
        public async Task EntityOnStateIncludeAttributesTriggerOnSameState()
        {
            // ARRANGE
            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "off");

            var cancelSource = DefaultHassClientMock.GetSourceWithTimeout();
            var triggered = false;

            // ACT
            DefaultDaemonHost
                .Entity("binary_sensor.pir")
                .WhenStateChange(allChanges: true)
                .Call((e, n, o) =>
                {
                    triggered = true;
                    return Task.CompletedTask;
                })
                .Execute();

            await RunDefauldDaemonUntilCanceled();

            // ASSERT
            Assert.True(triggered);
        }

        [Fact]
        public async Task TimerEveryShouldCallServiceCorrectNumberOfTimes()
        {
            // ARRANGE
            DefaultDaemonHost
                .Timer()
                .Every(TimeSpan.FromMilliseconds(20))
                .Entity("light.correct_light")
                .TurnOn()
                .Execute();

            await RunDefauldDaemonUntilCanceled(300);

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("turn_on", Times.AtLeast(4));
            DefaultHassClientMock.VerifyCallService("light", "turn_on", ("entity_id", "light.correct_light"));
        }

        [Fact]
        public async Task TimerEntitiesEveryShouldCallServiceCorrectNumberOfTimes()
        {
            // ARRANGE
            DefaultDaemonHost
                .Timer()
                .Every(TimeSpan.FromMilliseconds(20))
                .Entities(new string[] { "light.correct_light" })
                .TurnOn()
                .Execute();

            var cancelSource = new CancellationTokenSource(100); //DefaultHassClientMock.GetSourceWithTimeout(100);

            // ACTION
            await RunDefauldDaemonUntilCanceled();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("turn_on", Times.AtLeast(4));
            DefaultHassClientMock.VerifyCallService("light", "turn_on", ("entity_id", "light.correct_light"));
        }

        [Fact]
        public async Task TimerEntitiesLambdaEveryShouldCallServiceCorrectNumberOfTimes()
        {
            // ARRANGE
            DefaultDaemonHost.InternalState.Add("light.correct_light", new EntityState
            {
                EntityId = "light.correct_light"
            });

            DefaultDaemonHost
                .Timer()
                .Every(TimeSpan.FromMilliseconds(20))
                .Entities(n => n.EntityId == "light.correct_light")
                .TurnOn()
                .Execute();

            await RunDefauldDaemonUntilCanceled();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("turn_on", Times.AtLeast(4));
            DefaultHassClientMock.VerifyCallService("light", "turn_on", ("entity_id", "light.correct_light"));
        }

        [Fact]
        public async Task TimerEveryWithAttributeShouldCallServiceCorrectNumberOfTimes()
        {
            // ARRANGE
            DefaultDaemonHost
                .Timer()
                .Every(TimeSpan.FromMilliseconds(20))
                .Entity("light.correct_light")
                .TurnOn()
                    .UsingAttribute("attr", "on")
                .Execute();

            await RunDefauldDaemonUntilCanceled();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("turn_on", Times.AtLeast(4));
            DefaultHassClientMock.VerifyCallService("light", "turn_on", ("attr", "on"), ("entity_id", "light.correct_light"));
        }

        [Fact]
        public async Task TimerToggleShouldCallCorrectServiceCall()
        {
            // ARRANGE
            DefaultDaemonHost
                .Timer()
                .Every(TimeSpan.FromMilliseconds(20))
                .Entity("light.correct_light")
                .Toggle()
                .Execute();

            await RunDefauldDaemonUntilCanceled();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("toggle", Times.AtLeast(4));
            DefaultHassClientMock.VerifyCallService("light", "toggle", ("entity_id", "light.correct_light"));
        }

        [Fact]
        public async Task TimerTurnOffShouldCallCorrectServiceCall()
        {
            // ARRANGE
            DefaultDaemonHost
                .Timer()
                .Every(TimeSpan.FromMilliseconds(20))
                .Entity("light.correct_light")
                .TurnOff()
                .Execute();

            await RunDefauldDaemonUntilCanceled(200);

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("turn_off", Times.AtLeast(2));
            DefaultHassClientMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_light"));
        }

        [Fact]
        public async Task ToggleEntityCallsCorrectServiceCall()
        {
            // ARRANGE

            // ACT
            await DefaultDaemonHost
                .Entity("light.correct_entity")
                .Toggle()
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("toggle", Times.Once());
            DefaultHassClientMock.VerifyCallService("light", "toggle", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task TurnOffEntityCallsCorrectServiceCall()
        {
            // ARRANGE

            // ACT
            await DefaultDaemonHost
                .Entity("light.correct_entity")
                .TurnOff()
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("turn_off", Times.Once());
            DefaultHassClientMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task EntityFuncExceptionLogsError()
        {
            // ARRANGE
            DefaultDaemonHost.InternalState["id"] = new EntityState { EntityId = "id" };

            // ACT
            var x = await Assert.ThrowsAsync<Exception>(() => DefaultDaemonHost
               .Entities(n => throw new Exception("Some error"))
               .TurnOff()
               .ExecuteAsync());

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("turn_off", Times.Never());
            LoggerMock.AssertLogged(LogLevel.Error, Times.AtLeastOnce());
            Assert.Equal("Some error", x.Message);
        }


        [Fact]
        public async Task TurnOffEntityLambdaAttributeSelectionCallsCorrectServiceCall()
        {
            // ARRANGE
            var cancelSource = DefaultHassClientMock.GetSourceWithTimeout();
            await DefaultDaemonHost.Run("host", 8123, false, "token", cancelSource.Token);

            // ACT
            await DefaultDaemonHost
                .Entities(n => n?.Attribute?.test >= 100)
                .TurnOff()
                .ExecuteAsync();

            // ASSERT

            DefaultHassClientMock.VerifyCallServiceTimes("turn_off", Times.Exactly(3));
            DefaultHassClientMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity"));
            DefaultHassClientMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity2"));
            DefaultHassClientMock.VerifyCallService("switch", "turn_off", ("entity_id", "switch.correct_entity"));
        }

        [Fact]
        public async Task TurnOffEntityLambdaAttributeSelectionNoExistCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonHost
                .Entities(n => n?.Attribute?.not_exists == "test")
                .TurnOff()
                .ExecuteAsync();

            // ASSERT

            DefaultHassClientMock.VerifyCallServiceTimes("turn_off", Times.Never());
        }

        [Fact]
        public async Task TurnOffEntityLamdaSelectionCallsCorrectServiceCall()
        {
            // ARRANGE
            await RunDefauldDaemonUntilCanceled();

            // ACT
            await DefaultDaemonHost
                .Entities(n => n.EntityId.StartsWith("light.correct_entity"))
                .TurnOff()
                .ExecuteAsync();

            // ASSERT

            DefaultHassClientMock.VerifyCallServiceTimes("turn_off", Times.Exactly(2));
            DefaultHassClientMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity"));
            DefaultHassClientMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity2"));
        }

        [Fact]
        public async Task TurnOffLightLambdaAttributeSelectionCallsCorrectServiceCall()
        {
            // ARRANGE
            var cancelSource = DefaultHassClientMock.GetSourceWithTimeout();
            await DefaultDaemonHost.Run("host", 8123, false, "token", cancelSource.Token);

            // ACT
            await DefaultDaemonHost
                .Lights(n => n?.Attribute?.test >= 100)
                .TurnOff()
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("turn_off", Times.Exactly(2));
            DefaultHassClientMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity"));
            DefaultHassClientMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity2"));
        }

        [Fact]
        public async Task TurnOffLightWithoutDomainCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonHost
                .Light("correct_entity")
                .TurnOff()
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("turn_off", Times.Once());
            DefaultHassClientMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task TurnOffMultipleEntityCallsCorrectServiceCall()
        {
            // ARRANGE

            // ACT
            await DefaultDaemonHost
                .Entity("light.correct_entity", "light.correct_entity2")
                .TurnOff()
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("turn_off", Times.Exactly(2));

            DefaultHassClientMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity"));
            DefaultHassClientMock.VerifyCallService("light", "turn_off", ("entity_id", "light.correct_entity2"));
        }

        [Fact]
        public async Task TurnOnEntityCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonHost
                .Entity("light.correct_entity")
                .TurnOn()
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("turn_on", Times.Once());
            DefaultHassClientMock.VerifyCallService("light", "turn_on", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task TurnOnEntityWithAttributeCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonHost
                .Entity("light.correct_entity")
                .TurnOn()
                .WithAttribute("brightness", 100)
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("turn_on", Times.Once());
            DefaultHassClientMock.VerifyCallService("light", "turn_on",
                ("brightness", 100),
                ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task TurnOnEntityWithMultipleAttributeCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonHost
                .Entity("light.correct_entity")
                .TurnOn()
                .WithAttribute("brightness", 100)
                .WithAttribute("color_temp", 230)
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("turn_on", Times.Once());
            DefaultHassClientMock.VerifyCallService("light", "turn_on",
                ("brightness", 100),
                ("color_temp", 230),
                ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task SetStateEntityCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonHost
                .Entity("light.correct_entity")
                .SetState(50)
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifySetStateTimes("light.correct_entity", Times.Once());
            DefaultHassClientMock.VerifySetState("light.correct_entity", "50");
        }

        [Fact]
        public async Task SetStateWithAttributesEntityCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonHost
                .Entity("light.correct_entity")
                .SetState(50)
                .WithAttribute("attr1", "str_value")
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifySetStateTimes("light.correct_entity", Times.Once());
            DefaultHassClientMock.VerifySetState("light.correct_entity", "50", ("attr1", "str_value"));
        }

        [Fact]
        public async Task EntityOnStateCallFunction()
        {
            // ARRANGE
            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            var cancelSource = DefaultHassClientMock.GetSourceWithTimeout();

            string? actualToState = "";
            string? actualFromState = "";
            var actualEntity = "";
            DefaultDaemonHost
                .Entity("binary_sensor.pir")
                .WhenStateChange("on").Call((entity, to, from) =>
                {
                    actualToState = to?.State;
                    actualFromState = from?.State;
                    actualEntity = entity;
                    return Task.CompletedTask;
                }).Execute();

            await RunDefauldDaemonUntilCanceled();

            Assert.Equal("on", actualToState);
            Assert.Equal("off", actualFromState);
            Assert.Equal("binary_sensor.pir", actualEntity);
        }

        [Fact]
        public async Task EntityOnStateTriggerScript()
        {
            // ARRANGE
            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "on", "off");

            var cancelSource = DefaultHassClientMock.GetSourceWithTimeout();

            // ACT
            DefaultDaemonHost
                .Entity("binary_sensor.pir")
                .WhenStateChange(to: "off")
                .RunScript("thescript")
                .Execute();

            await RunDefauldDaemonUntilCanceled();

            DefaultHassClientMock.Verify(n => n.CallService("script", "thescript", It.IsAny<object>(), false));
        }

        [Fact]
        public async Task SpeakCallsCorrectServiceCall()
        {
            // ARRANGE
            DefaultDaemonHost.InternalDelayTimeForTts = 0; // For testing

            // ACT
            await DefaultDaemonHost
                .MediaPlayer("media_player.correct_player")
                .Speak("a message")
                .ExecuteAsync();

            var (daemonTask, _) = ReturnRunningDefauldDaemonHostTask();

            await Task.Delay(20);

            var expObject = new ExpandoObject();
            dynamic expectedAttruibutes = expObject;
            expectedAttruibutes.entity_id = "media_player.correct_player";
            expectedAttruibutes.message = "a message";

            // ASSERT
            DefaultHassClientMock.Verify(n => n.CallService("tts", "google_cloud_say", expObject, true));

            await WaitUntilCanceled(daemonTask);
        }

        [Fact]
        public async Task MediaPlayerPlayCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonHost
                .MediaPlayer("media_player.player")
                .Play()
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("media_play", Times.Once());
            DefaultHassClientMock.VerifyCallService("media_player", "media_play", ("entity_id", "media_player.player"));
        }

        [Fact]
        public async Task MediaPlayersFuncPlayCallsCorrectServiceCall()
        {
            // ARRANGE
            DefaultDaemonHost.InternalState["media_player.player"] = new EntityState
            {
                EntityId = "media_player.player",
                State = "off"
            };

            // ACT
            await DefaultDaemonHost
                    .MediaPlayers(n => n.EntityId == "media_player.player")
                    .Play()
                    .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("media_play", Times.Once());
            DefaultHassClientMock.VerifyCallService("media_player", "media_play", ("entity_id", "media_player.player"));
        }

        [Fact]
        public async Task MediaPlayersFuncExceptionLogsError()
        {
            // ARRANGE
            DefaultDaemonHost.InternalState["id"] = new EntityState { EntityId = "id" };

            // ACT
            var x = await Assert.ThrowsAsync<Exception>(() => DefaultDaemonHost
               .MediaPlayers(n => throw new Exception("Some error"))
               .Play()
               .ExecuteAsync());

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("turn_off", Times.Never());
            LoggerMock.AssertLogged(LogLevel.Error, Times.AtLeastOnce());
            Assert.Equal("Some error", x.Message);
        }


        [Fact]
        public async Task MediaPlayersPlayCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonHost
                .MediaPlayers(new string[] { "media_player.player" })
                .Play()
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("media_play", Times.Once());
            DefaultHassClientMock.VerifyCallService("media_player", "media_play", ("entity_id", "media_player.player"));
        }

        [Fact]
        public async Task MediaPlayerPauseCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonHost
                .MediaPlayer("media_player.player")
                .Pause()
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("media_pause", Times.Once());
            DefaultHassClientMock.VerifyCallService("media_player", "media_pause", ("entity_id", "media_player.player"));
        }

        [Fact]
        public async Task MediaPlayerPlayPauseCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonHost
                .MediaPlayer("media_player.player")
                .PlayPause()
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("media_play_pause", Times.Once());
            DefaultHassClientMock.VerifyCallService("media_player", "media_play_pause", ("entity_id", "media_player.player"));
        }

        [Fact]
        public async Task MediaPlayerStopCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonHost
                .MediaPlayer("media_player.player")
                .Stop()
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("media_stop", Times.Once());
            DefaultHassClientMock.VerifyCallService("media_player", "media_stop", ("entity_id", "media_player.player"));
        }
    }
}