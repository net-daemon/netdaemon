using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.AppModel;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetDaemon.Tests.Integration.Helpers;
using Xunit;

namespace NetDaemon.Tests.Integration;

[NetDaemonApp]
public class BasicApp
{
    public BasicApp(
        IHaContext haContext
    )
    {
        haContext.StateChanges()
            .Where(n =>
                n.Entity.EntityId == "input_select.who_cooks"
            )
            .Subscribe(
                s => haContext.CallService("input_text", "set_value",
                    ServiceTarget.FromEntities("input_text.test_result"), new {value = s.New?.State})
            );
    }
}

public class BasicTests : NetDaemonIntegrationBase
{
    [Fact]
    public async Task BasicTestApp_ShouldChangeStateOfInputTextToTheStateOfInputSelect_WhenChange()
    {
        var haContext = Services.GetRequiredService<IHaContext>();
        var optionToSet = GetDifferentOptionThanCurrentlySelected(haContext);

        var waitTask = haContext.StateChanges()
            .Where(n => n.Entity.EntityId == "input_text.test_result")
            .FirstAsync()
            .ToTask();

        haContext.CallService(
            "input_select",
            "select_option",
            ServiceTarget.FromEntities("input_select.who_cooks"),
            new {option = optionToSet});

        var act = async () => await waitTask.ConfigureAwait(false);

        var result = (await act.Should().CompleteWithinAsync(5000.Milliseconds())).Subject;
        result.New!.State.Should().Be(optionToSet);
    }

    private static string GetDifferentOptionThanCurrentlySelected(IHaContext haContext)
    {
        var currentState = haContext.GetState("input_select.who_cooks")?.State
                           ?? throw new InvalidOperationException();

        var useOption = currentState switch
        {
            "Paulus" => "Anne Therese",
            _ => "Paulus"
        };
        return useOption;
    }

    public BasicTests(HomeAssistantLifetime homeAssistantLifetime) : base(homeAssistantLifetime)
    {
    }
}
