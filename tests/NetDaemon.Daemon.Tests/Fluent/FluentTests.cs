using System;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NetDaemon.Common;
using NetDaemon.Common.Fluent;
using NetDaemon.Daemon.Fakes;
using Xunit;

namespace NetDaemon.Daemon.Tests.Fluent
{
    /// <summary>
    ///     Tests the fluent API parts of the daemon
    /// </summary>
    /// <remarks>
    ///     Mainly the tests checks if correct underlying call to "CallService"
    ///     has been made.
    /// </remarks>
    public class FluentTests : CoreDaemonHostTestBase
    {
        public FluentTests() : base()
        {
        }

        [Fact]
        public async Task EntityOnStateChangedForTimeTurnOffLightCallsCorrectServiceCall()
        {
            // ARRANGE

            var lastChanged = new DateTime(2020, 1, 1, 1, 1, 1, 20);
            var lastUpdated = new DateTime(2020, 1, 1, 1, 1, 1, 50);

            await FakeDaemonInit(300).ConfigureAwait(false);

            DefaultDaemonApp
                .Entity("binary_sensor.pir")
                .WhenStateChange("off")
                .AndNotChangeFor(TimeSpan.FromMilliseconds(100))
                .UseEntity("light.correct_entity")
                .TurnOff()
                .Execute();

            AddChangedEvent("binary_sensor.pir", "on", "off",
            lastUpdated, lastChanged);

            // ASSERT
            await Task.Delay(10); // After 10ms we should not have call
            VerifyCallServiceTimes("turn_off", Times.Never());
            await Task.Delay(300); // After 30ms we should have call
            VerifyCallServiceTimes("turn_off", Times.Once());
            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);
        }

        [Fact]
        public async Task EntityOnStateChangedLamdaTurnOnLightCallsCorrectServiceCall()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            AddChangedEvent("binary_sensor.pir", "off", "on");

            DefaultDaemonApp
                .Entity("binary_sensor.pir")
                .WhenStateChange((n, _) => n?.State == "on")
                .UseEntity("light.correct_entity")
                .TurnOn()
                .Execute();

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            VerifyCallServiceTimes("turn_on", Times.Once());
            VerifyCallServiceTuple("light", "turn_on", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task EntityOnStateChangedLamdaWithMultipleEntitiesCallsCorrectServiceCall()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            AddChangedEvent("binary_sensor.pir", "off", "on");

            var MotionEnabled = true;

            DefaultDaemonApp
                .Entities(new string[] { "binary_sensor.pir", "binary_sensor-pir2" })
                .WhenStateChange((to, from) => @from?.State == "off" && to?.State == "on" && MotionEnabled)
                .UseEntity("light.correct_entity")
                .Toggle()
                .Execute();

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            VerifyCallServiceTimes("toggle", Times.Once());
            VerifyCallServiceTuple("light", "toggle", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task OneEntityWithSeveralShouldCallsCorrectServiceCall()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            AddChangedEvent("binary_sensor.pir", "off", "on");

            DefaultDaemonApp
                .Entity("binary_sensor.pir")
                .WhenStateChange((n, _) => n?.State == "on")
                .UseEntity("light.correct_entity")
                .Toggle()
                .Execute();

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            VerifyCallServiceTimes("toggle", Times.Once());
            VerifyCallServiceTuple("light", "toggle", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task EntityOnStateChangedMultipleTimesCallsCorrectServiceCall()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);

            AddChangedEvent("binary_sensor.pir", "off", "on");
            AddChangedEvent("binary_sensor.pir", "off", "on");

            DefaultDaemonApp
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .UseEntity("light.correct_entity")
                .TurnOn()
                .Execute();

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);
            VerifyCallServiceTimes("turn_on", Times.Exactly(2));
            VerifyCallServiceTuple("light", "turn_on",
                ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task EntityOnStateChangedTurnOnLightCallsCorrectServiceCall()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            AddChangedEvent("binary_sensor.pir", "off", "on");

            DefaultDaemonApp
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .UseEntity("light.correct_entity")
                .TurnOn()
                .Execute();

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            VerifyCallServiceTimes("turn_on", Times.Once());
            VerifyCallServiceTuple("light", "turn_on", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task EntityOnStateChangedEntitiesTurnOnLightCallsCorrectServiceCall()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            AddChangedEvent("binary_sensor.pir", "off", "on");

            DefaultDaemonApp
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .UseEntities(new string[] { "light.correct_entity" })
                .TurnOn()
                .Execute();

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            VerifyCallServiceTimes("turn_on", Times.Once());
            VerifyCallServiceTuple("light", "turn_on", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task EntityOnStateChangedEntitiesLambdaTurnOnLightCallsCorrectServiceCall()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);

            AddChangedEvent("binary_sensor.pir", "off", "on");


            // Fake the
            DefaultDaemonHost.InternalState["light.correct_entity"] = new EntityState
            {
                EntityId = "light.correct_entity"
            };

            DefaultDaemonApp
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .UseEntities(n => n.EntityId == "light.correct_entity")
                .TurnOn()
                .Execute();

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            VerifyCallServiceTimes("turn_on", Times.Once());
            VerifyCallServiceTuple("light", "turn_on", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task EntityOnStateChangedTurnOnLightCallsCorrectServiceCallButNoTurnOff()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);

            AddChangedEvent("binary_sensor.pir", "off", "on");

            DefaultDaemonApp
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .UseEntity("light.correct_entity")
                .TurnOn()
                .Execute();

            DefaultDaemonApp
                .Entity("binary_sensor.pir")
                .WhenStateChange("off")
                .UseEntity("light.correct_entity")
                .TurnOff()
                .Execute();

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            VerifyCallServiceTimes("turn_on", Times.Once());
            VerifyCallServiceTuple("light", "turn_on", ("entity_id", "light.correct_entity"));

            VerifyCallServiceTimes("turn_off", Times.Never());
        }

        [Fact]
        public async Task EntityOnStateChangedTurnOnLightWithAttributesCallsCorrectServiceCall()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            AddChangedEvent("binary_sensor.pir", "off", "on");

            DefaultDaemonApp
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .UseEntity("light.correct_entity")
                .TurnOn()
                .WithAttribute("transition", 0)
                .Execute();

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            VerifyCallServiceTimes("turn_on", Times.Once());
            VerifyCallServiceTuple("light", "turn_on",
                ("transition", 0),
                ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task EntityOnStateDefaultTriggerOnAnyStateChange()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            AddChangedEvent("binary_sensor.pir", "off", "on");

            var triggered = false;

            // ACT
            DefaultDaemonApp
                .Entity("binary_sensor.pir")
                .WhenStateChange()
                .Call((e, n, o) =>
                {
                    triggered = true;
                    return Task.CompletedTask;
                })
                .Execute();

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);
            // ASSERT
            Assert.True(triggered);
        }

        [Fact]
        public async Task EntityOnStateNotTriggerOnSameState()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            AddChangedEvent("binary_sensor.pir", "off", "off");

            var triggered = false;

            // ACT
            DefaultDaemonApp
                .Entity("binary_sensor.pir")
                .WhenStateChange()
                .Call((e, n, o) =>
                {
                    triggered = true;
                    return Task.CompletedTask;
                })
                .Execute();

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.False(triggered);
        }

        [Fact]
        public async Task EntityOnStateIncludeAttributesTriggerOnSameState()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            AddChangedEvent("binary_sensor.pir", "off", "off");

            var triggered = false;

            // ACT
            DefaultDaemonApp
                .Entity("binary_sensor.pir")
                .WhenStateChange(allChanges: true)
                .Call((e, n, o) =>
                {
                    triggered = true;
                    return Task.CompletedTask;
                })
                .Execute();

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);
            // ASSERT
            Assert.True(triggered);
        }

        [Fact]
        public async Task ToggleEntityCallsCorrectServiceCall()
        {
            // ARRANGE

            // ACT
            await DefaultDaemonApp
                .Entity("light.correct_entity")
                .Toggle()
                .ExecuteAsync();

            // ASSERT
            VerifyCallServiceTimes("toggle", Times.Once());
            VerifyCallServiceTuple("light", "toggle", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task TurnOffEntityCallsCorrectServiceCall()
        {
            // ARRANGE

            // ACT
            await DefaultDaemonApp
                .Entity("light.correct_entity")
                .TurnOff()
                .ExecuteAsync();

            // ASSERT
            VerifyCallServiceTimes("turn_off", Times.Once());
            VerifyCallServiceTuple("light", "turn_off", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task EntityFuncExceptionLogsError()
        {
            // ARRANGE
            DefaultDaemonHost.InternalState["id"] = new EntityState { EntityId = "id" };

            // ACT
            var x = await Assert.ThrowsAsync<Exception>(() => DefaultDaemonApp
               .Entities(n => throw new Exception("Some error"))
               .TurnOff()
               .ExecuteAsync());

            // ASSERT
            VerifyCallServiceTimes("turn_off", Times.Never());
            LoggerMock.AssertLogged(LogLevel.Error, Times.AtLeastOnce());
            Assert.Equal("Some error", x.Message);
        }

        [Fact]
        public async Task TurnOffEntityLambdaAttributeSelectionCallsCorrectServiceCall()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);

            // ACT
            await DefaultDaemonApp
                .Entities(n => n?.Attribute?.test >= 100)
                .TurnOff()
                .ExecuteAsync();

            // ASSERT

            VerifyCallServiceTimes("turn_off", Times.Exactly(3));
            VerifyCallServiceTuple("light", "turn_off", ("entity_id", "light.correct_entity"));
            VerifyCallServiceTuple("light", "turn_off", ("entity_id", "light.correct_entity2"));
            VerifyCallServiceTuple("switch", "turn_off", ("entity_id", "switch.correct_entity"));

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);
        }

        [Fact]
        public async Task TurnOffEntityLambdaAttributeSelectionNoExistCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonApp
                .Entities(n => n?.Attribute?.not_exists == "test")
                .TurnOff()
                .ExecuteAsync();

            // ASSERT

            VerifyCallServiceTimes("turn_off", Times.Never());
        }

        [Fact]
        public async Task TurnOffEntityLamdaSelectionCallsCorrectServiceCall()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            // ACT
            await DefaultDaemonApp
                .Entities(n => n.EntityId.StartsWith("light.correct_entity"))
                .TurnOff()
                .ExecuteAsync();

            // ASSERT

            VerifyCallServiceTimes("turn_off", Times.Exactly(2));
            VerifyCallServiceTuple("light", "turn_off", ("entity_id", "light.correct_entity"));
            VerifyCallServiceTuple("light", "turn_off", ("entity_id", "light.correct_entity2"));

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);
        }

        [Fact]
        public async Task TurnOffMultipleEntityCallsCorrectServiceCall()
        {
            // ARRANGE

            // ACT
            await DefaultDaemonApp
                .Entity("light.correct_entity", "light.correct_entity2")
                .TurnOff()
                .ExecuteAsync();

            // ASSERT
            VerifyCallServiceTimes("turn_off", Times.Exactly(2));

            VerifyCallServiceTuple("light", "turn_off", ("entity_id", "light.correct_entity"));
            VerifyCallServiceTuple("light", "turn_off", ("entity_id", "light.correct_entity2"));
        }

        [Fact]
        public async Task TurnOnEntityCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonApp
                .Entity("light.correct_entity")
                .TurnOn()
                .ExecuteAsync();

            // ASSERT
            VerifyCallServiceTimes("turn_on", Times.Once());
            VerifyCallServiceTuple("light", "turn_on", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task TurnOnEntityWithAttributeCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonApp
                .Entity("light.correct_entity")
                .TurnOn()
                .WithAttribute("brightness", 100)
                .ExecuteAsync();

            // ASSERT
            VerifyCallServiceTimes("turn_on", Times.Once());
            VerifyCallServiceTuple("light", "turn_on",
                ("brightness", 100),
                ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task TurnOnEntityWithMultipleAttributeCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonApp
                .Entity("light.correct_entity")
                .TurnOn()
                .WithAttribute("brightness", 100)
                .WithAttribute("color_temp", 230)
                .ExecuteAsync();

            // ASSERT
            VerifyCallServiceTimes("turn_on", Times.Once());
            VerifyCallServiceTuple("light", "turn_on",
                ("brightness", 100),
                ("color_temp", 230),
                ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task SetStateEntityCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonApp
                .Entity("light.correct_entity")
                .SetState(50)
                .ExecuteAsync();

            // ASSERT
            VerifySetStateTimes("light.correct_entity", Times.Once());
            VerifySetState("light.correct_entity", "50");
        }

        [Fact]
        public async Task SetStateWithAttributesEntityCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonApp
                .Entity("light.correct_entity")
                .SetState(50)
                .WithAttribute("attr1", "str_value")
                .ExecuteAsync();

            // ASSERT
            VerifySetStateTimes("light.correct_entity", Times.Once());
            VerifySetState("light.correct_entity", "50", ("attr1", "str_value"));
        }

        [Fact]
        public async Task EntityOnStateCallFunction()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            AddChangedEvent("binary_sensor.pir", "off", "on");

            string? actualToState = "";
            string? actualFromState = "";
            var actualEntity = "";
            DefaultDaemonApp
                .Entity("binary_sensor.pir")
                .WhenStateChange("on").Call((entity, to, from) =>
                {
                    actualToState = to?.State;
                    actualFromState = from?.State;
                    actualEntity = entity;
                    return Task.CompletedTask;
                }).Execute();

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            Assert.Equal("on", actualToState);
            Assert.Equal("off", actualFromState);
            Assert.Equal("binary_sensor.pir", actualEntity);
        }

        [Fact]
        public async Task EntityOnStateTriggerScript()
        {
            await FakeDaemonInit().ConfigureAwait(false);
            // ARRANGE
            AddChangedEvent("binary_sensor.pir", "on", "off");

            // ACT
            DefaultDaemonApp
                .Entity("binary_sensor.pir")
                .WhenStateChange(to: "off")
                .RunScript("thescript")
                .Execute();

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            VerifyCallService("script", "thescript", false);
            // Verify(n => n.CallService("script", "thescript", It.IsAny<object>(), false));
        }

        [Fact]
        public async Task SpeakCallsCorrectServiceCall()
        {
            await FakeDaemonInit().ConfigureAwait(false);
            // ARRANGE
            DefaultDaemonHost.InternalDelayTimeForTts = 0; // For testing

            // ACT
            await DefaultDaemonApp
                .MediaPlayer("media_player.correct_player")
                .Speak("a message")
                .ExecuteAsync();

            await Task.Delay(20);

            var expObject = new FluentExpandoObject();
            dynamic expectedAttruibutes = expObject;
            expectedAttruibutes.entity_id = "media_player.correct_player";
            expectedAttruibutes.message = "a message";

            // ASSERT
            VerifyCallService("tts", "google_cloud_say", expObject, true);
            // Verify(n => n.CallService("tts", "google_cloud_say", expObject, true));

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);
        }

        [Fact]
        public async Task MediaPlayerPlayCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonApp
                .MediaPlayer("media_player.player")
                .Play()
                .ExecuteAsync();

            // ASSERT
            VerifyCallServiceTimes("media_play", Times.Once());
            VerifyCallServiceTuple("media_player", "media_play", ("entity_id", "media_player.player"));
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
            await DefaultDaemonApp
                    .MediaPlayers(n => n.EntityId == "media_player.player")
                    .Play()
                    .ExecuteAsync();

            // ASSERT
            VerifyCallServiceTimes("media_play", Times.Once());
            VerifyCallServiceTuple("media_player", "media_play", ("entity_id", "media_player.player"));
        }

        [Fact]
        public async Task MediaPlayersFuncExceptionLogsError()
        {
            // ARRANGE
            DefaultDaemonHost.InternalState["id"] = new EntityState { EntityId = "id" };

            // ACT
            var x = await Assert.ThrowsAsync<Exception>(() => DefaultDaemonApp
               .MediaPlayers(n => throw new Exception("Some error"))
               .Play()
               .ExecuteAsync());

            // ASSERT
            VerifyCallServiceTimes("turn_off", Times.Never());
            LoggerMock.AssertLogged(LogLevel.Error, Times.AtLeastOnce());
            Assert.Equal("Some error", x.Message);
        }

        [Fact]
        public async Task MediaPlayersPlayCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonApp
                .MediaPlayers(new string[] { "media_player.player" })
                .Play()
                .ExecuteAsync();

            // ASSERT
            VerifyCallServiceTimes("media_play", Times.Once());
            VerifyCallServiceTuple("media_player", "media_play", ("entity_id", "media_player.player"));
        }

        [Fact]
        public async Task MediaPlayerPauseCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonApp
                .MediaPlayer("media_player.player")
                .Pause()
                .ExecuteAsync();

            // ASSERT
            VerifyCallServiceTimes("media_pause", Times.Once());
            VerifyCallServiceTuple("media_player", "media_pause", ("entity_id", "media_player.player"));
        }

        [Fact]
        public async Task MediaPlayerPlayPauseCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonApp
                .MediaPlayer("media_player.player")
                .PlayPause()
                .ExecuteAsync();

            // ASSERT
            VerifyCallServiceTimes("media_play_pause", Times.Once());
            VerifyCallServiceTuple("media_player", "media_play_pause", ("entity_id", "media_player.player"));
        }

        [Fact]
        public async Task MediaPlayerStopCallsCorrectServiceCall()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonApp
                .MediaPlayer("media_player.player")
                .Stop()
                .ExecuteAsync();

            // ASSERT
            VerifyCallServiceTimes("media_stop", Times.Once());
            VerifyCallServiceTuple("media_player", "media_stop", ("entity_id", "media_player.player"));
        }

        [Fact]
        public async Task InputSelectSetOptionShouldCallCorrectCallService()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonApp
                .InputSelect("input_select.myselect")
                .SetOption("option1")
                .ExecuteAsync();

            // ASSERT
            VerifyCallServiceTimes("select_option", Times.Once());
            VerifyCallServiceTuple("input_select", "select_option",
                ("entity_id", "input_select.myselect"),
                ("option", "option1"));
        }

        [Fact]
        public async Task InputSelectSetOptionIEnumerableShouldCallCorrectCallService()
        {
            // ARRANGE
            // ACT
            await DefaultDaemonApp
                .InputSelects(new string[] { "input_select.myselect" })
                .SetOption("option1")
                .ExecuteAsync();

            // ASSERT
            VerifyCallServiceTimes("select_option", Times.Once());
            VerifyCallServiceTuple("input_select", "select_option",
                ("entity_id", "input_select.myselect"),
                ("option", "option1"));
        }

        [Fact]
        public async Task InputSelectSetOptionFuncShouldCallCorrectCallService()
        {
            // ARRANGE
            DefaultDaemonHost.InternalState["input_select.myselect"] = new EntityState { EntityId = "input_select.myselect" };
            // ACT
            await DefaultDaemonApp
                .InputSelects(n => n.EntityId == "input_select.myselect")
                .SetOption("option1")
                .ExecuteAsync();

            // ASSERT
            VerifyCallServiceTimes("select_option", Times.Once());
            VerifyCallServiceTuple("input_select", "select_option",
                ("entity_id", "input_select.myselect"),
                ("option", "option1"));
        }

        [Fact]
        public async Task EntityDelayUntilStateChangeShouldReturnTrue()
        {
            // ARRANGE

            await FakeDaemonInit().ConfigureAwait(false);
            AddChangedEvent("binary_sensor.pir", "off", "on");

            var delayResult = DefaultDaemonApp
                .Entity("binary_sensor.pir")
                .DelayUntilStateChange(to: "on");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            Assert.True(delayResult.Task.IsCompletedSuccessfully);
            Assert.True(delayResult.Task.Result);
        }

        [Fact]
        public async Task EntityDelayUntilStateChangeLamdaShouldReturnTrue()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);

            AddChangedEvent("binary_sensor.pir", "off", "on");

            var delayResult = DefaultDaemonApp
                .Entity("binary_sensor.pir")
                .DelayUntilStateChange((to, _) => to?.State == "on");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            Assert.True(delayResult.Task.IsCompletedSuccessfully);
            Assert.True(delayResult.Task.Result);
        }

        [Fact]
        public async Task EntityInAreaOnStateChangedTurnOnLight()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);

            // Fake the state
            SetEntityState("light.ligth_in_area", area: "Area");

            // ACT
            DefaultDaemonApp
                .Entities(n => n.Area == "Area")
                .WhenStateChange("on")
                .UseEntity("light.correct_entity")
                .TurnOn()
                .Execute();

            // light.light_in_area is setup so it has area = Area
            AddChangedEvent("light.ligth_in_area", "off", "on");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // ASSERT
            VerifyCallServiceTimes("turn_on", Times.Once());
            VerifyCallServiceTuple("light", "turn_on", ("entity_id", "light.correct_entity"));
        }

        [Fact]
        public async Task EntityInAreaOnStateChangedShouldTurnOn()
        {
            // ARRANGE

            await FakeDaemonInit().ConfigureAwait(false);

            // Fake the state
            SetEntityState("light.ligth_in_area", area: "Area");

            // ACT
            DefaultDaemonApp
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .UseEntities(n => n.Area == "Area")
                .TurnOn()
                .Execute();

            // light.light_in_area is setup so it has area = Area
            AddChangedEvent("binary_sensor.pir", "off", "on");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);
            // ASSERT
            VerifyCallServiceTimes("turn_on", Times.Once());
            VerifyCallServiceTuple("light", "turn_on", ("entity_id", "light.ligth_in_area"));
        }

        [Fact]
        public async Task RunScriptShouldCallCorrectService()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);

            // ACT
            await DefaultDaemonApp.RunScript("testscript").ExecuteAsync();
            await DefaultDaemonApp.RunScript("script.testscript").ExecuteAsync();

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);
            // ASSERT
            VerifyCallServiceTimes("testscript", Times.Exactly(2));
        }

        [Fact]
        public async Task SendEventShouldCallCorrectService()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);

            // ACT
            await DefaultDaemonApp.SendEvent("myevent");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);
            // ASSERT
            VerifyEventSent("myevent");
        }
    }

}