using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Extensions;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.HassModel;
using NetDaemon.Tests.Integration.Helpers;
using Xunit;

namespace NetDaemon.Tests.Integration;

public class HelpersTests(HomeAssistantLifetime homeAssistantLifetime) : NetDaemonIntegrationBase(homeAssistantLifetime)
{
    /// <summary>
    ///    Tests the CRUD operations of number helper
    /// </summary>
    /// <remarks>
    ///      We need ot test these operations in serial since we cannont rely on test order
    /// </remarks>
    [Fact]
    public async Task HelpersTest_CRUD_Should_WorkOnHelpers()
    {
        var cancelToken = new CancellationTokenSource(5000).Token;
        var conn = Services.GetRequiredService<IHomeAssistantConnection>();

        var helper = await conn.CreateInputNumberHelperAsync(
            name: "MyNumberHelper",
            min: 0,
            max: 100.0,
            step: 1.2,
            initial: 10.0,
            unitOfMeasurement: "ml",
            mode: "slider",
            cancelToken
        );

        helper.Should().NotBeNull();

        helper!.Id.Should().NotBeNullOrEmpty();
        helper.Name.Should().Be("MyNumberHelper");
        helper.Min.Should().Be(0);
        helper.Max.Should().Be(100);
        helper.Step.Should().Be(1.2);
        helper.Initial.Should().Be(10.0);
        helper.UnitOfMeasurement.Should().Be("ml");
        helper.Mode.Should().Be("slider");

        // Now lets test the list operation to make sure the helper is there
        var list = await conn.ListInputNumberHelpersAsync(cancelToken);
        list.Should().NotBeNullOrEmpty();
        list.Should().ContainEquivalentOf(helper);

        // The following test is just for making sure the helper can be used in practice
        // by incrementing and check the state behing changed correctly by increasing the correct amount
        var haContext = Services.GetRequiredService<IHaContext>();
        var waitTask = haContext.StateChanges()
            .Where(n => n.Entity.EntityId == "input_number.mynumberhelper")
            .FirstAsync()
            .ToTask();

        await conn.CallServiceAsync("input_number", "increment", null, new HassTarget { EntityIds = ["input_number.mynumberhelper"] }, cancelToken);

        var act = async () => await waitTask.ConfigureAwait(false);
        var result = (await act.Should().CompleteWithinAsync(5000.Milliseconds())).Subject;

        result.New!.State.Should().Be("11.2");

        // Delete the helper and make sure it is not listed anymore
        await conn.DeleteInputNumberHelperAsync(helper.Id, cancelToken);

        list = await conn.ListInputNumberHelpersAsync(cancelToken);

        // This list should be null but incase there are other tests running in parallel in the future
        // just make sure the helper is not there
        list?.Should().NotContainEquivalentOf(helper);
    }
}
