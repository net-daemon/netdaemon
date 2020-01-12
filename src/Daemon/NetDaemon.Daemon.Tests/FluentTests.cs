using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using Xunit;

namespace NetDaemon.Daemon.Tests
{
    public class FluentTests
    {
        [Fact]
        public async Task ActionWhenTurnOffCheckCallServiceIsCalledWithCorrectId()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            await daemonHost
                .Action
                    .TurnOff
                        .Entity("light.correct_entity")
                .ExecuteAsync();

            // ASSERT
            var attributes = new ExpandoObject();
            ((IDictionary<string, object>)attributes)["entity_id"] = "light.correct_entity";
            hcMock.Verify(n => n.CallService("light", "turn_off", attributes));
        }

        [Fact]
        public async Task ActionWhenTurnOnWithoutAttributesCheckCallServiceIsCalledWithCorrectId()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            await daemonHost
                .Action
                    .TurnOn
                        .Entity("light.correct_entity")
                .ExecuteAsync();

            // ASSERT
            var attributes = new ExpandoObject();
            ((IDictionary<string, object>)attributes)["entity_id"] = "light.correct_entity";
            hcMock.Verify(n => n.CallService("light", "turn_on", attributes));
        }

        [Fact]
        public async Task ActionWhenTurnOnWithAttributesCheckCallServiceIsCalledWithCorrectId()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            await daemonHost
                .Action
                    .TurnOn
                        .Entity("light.correct_entity")
                            .UsingAttribute("brightness", 123)
                            .UsingAttribute("transition", 10)
                .ExecuteAsync();

            // ASSERT
            var attributes = new ExpandoObject();
            // The order matters for the verify to match
            ((IDictionary<string, object>)attributes)["brightness"] = 123;
            ((IDictionary<string, object>)attributes)["transition"] = 10;
            ((IDictionary<string, object>)attributes)["entity_id"] = "light.correct_entity";

            hcMock.Verify(n => n.CallService("light", "turn_on", attributes));
        }

        [Fact]
        public async Task ActionWhenToggleWithAttributesCheckCallServiceIsCalledWithCorrectId()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            await daemonHost
                .Action
                    .Toggle
                        .Entity("light.correct_entity")
                            .UsingAttribute("brightness", 123)
                            .UsingAttribute("transition", 10)
                    .ExecuteAsync();

            // ASSERT
            var attributes = new ExpandoObject();
            // The order matters for the verify to match
            ((IDictionary<string, object>)attributes)["brightness"] = 123;
            ((IDictionary<string, object>)attributes)["transition"] = 10;
            ((IDictionary<string, object>)attributes)["entity_id"] = "light.correct_entity";

            hcMock.Verify(n => n.CallService("light", "toggle", attributes));
        }

        [Fact]
        public async Task ActionWhenTurnOnMultipleEntitiesWithAttributesCallServiceIsOk()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            await daemonHost
                .Action
                .TurnOn
                    .Entities("light.correct_entity", "light.another_correct_entity")
                        .UsingAttribute("brightness", 123)
                        .UsingAttribute("transition", 10)
                .ExecuteAsync();

            // ASSERT

            // Check first call
            var attributes = new ExpandoObject();
            // The order matters for the verify to match
            ((IDictionary<string, object>)attributes)["brightness"] = 123;
            ((IDictionary<string, object>)attributes)["transition"] = 10;
            ((IDictionary<string, object>)attributes)["entity_id"] = "light.correct_entity";

            hcMock.Verify(n => n.CallService("light", "turn_on", attributes));

            // Check second call
            attributes = new ExpandoObject();
            // The order matters for the verify to match
            ((IDictionary<string, object>)attributes)["brightness"] = 123;
            ((IDictionary<string, object>)attributes)["transition"] = 10;
            ((IDictionary<string, object>)attributes)["entity_id"] = "light.another_correct_entity";

            hcMock.Verify(n => n.CallService("light", "turn_on", attributes));
        }

        [Fact]
        public async Task EntityWhenMalformattedEntityThrowsApplicationException()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT and ASSERT
            await Assert.ThrowsAsync<ApplicationException>(async () =>
                await daemonHost.Action.TurnOff.Entity("light!error").ExecuteAsync());
        }
    }
}
