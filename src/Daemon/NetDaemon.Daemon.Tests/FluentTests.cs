using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using Xunit;
using System.Linq;
using System.Linq.Expressions;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.NetDaemon.Common;

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

        //[Fact]
        //public void DoIt() => Expr(n => n.State=="2" && n.Attribute.Test ==2  );
        //private void Expr(Func<EntityState, bool> expr)
        //{
        //    var par = expr.Method.GetParameters();

        //    var xx = expr.Method.GetMethodBody().LocalVariables;
        //    dynamic x = new ExpandoObject();

        //    x.State = 2;
        //    var c = expr.Invoke(x);
        //}

        [Fact]
        public async Task ActionWhenTurnOnWithLambdaSelectAttributesCheckCallServiceIsCalledWithCorrectId()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // Use the default states
            // The test should only resturn the light with test attribute >= 100
            // ACT
            await daemonHost
            .Action
                .TurnOn
                    .Entity("light.correct_entity")
                        .UsingAttribute("brightness", 123)
                    .Entity("light.correct_entity2")
                        .UsingAttribute("brightness", 321)
                    .Entity("light.filtered_entity") // Has attribute test==90
                    .Where(n=> n.Attribute.test >= 100)
            .ExecuteAsync();

            // ASSERT we get the two calls that has test attribute >= 100
            var attributes = new ExpandoObject();
            ((IDictionary<string, object>)attributes)["brightness"] = 123;
            ((IDictionary<string, object>)attributes)["entity_id"] = "light.correct_entity";
            hcMock.Verify(n => n.CallService("light", "turn_on", attributes));

            attributes = new ExpandoObject();
            ((IDictionary<string, object>)attributes)["brightness"] = 321;
            ((IDictionary<string, object>)attributes)["entity_id"] = "light.correct_entity2";
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

        [Fact]
        public async Task TestLinqTask()
        {
            var x = new List<string>();
      
        }
    }
}
