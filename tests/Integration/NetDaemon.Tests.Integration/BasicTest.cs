using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using FluentAssertions;
using NetDaemon.AppModel;
using NetDaemon.HassModel.Common;
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

public class BasicTests : IClassFixture<MakeSureNetDaemonIsRunningFixture>
{
    private readonly IHaContext _haContext;

    public BasicTests(
        MakeSureNetDaemonIsRunningFixture _,
        IHaContext haContext
    )
    {
        _haContext = haContext;
    }

    [Fact]
    public async Task BasicTestApp_ShouldChangeStateOfInputTextToTheStateOfInputSelect_WhenChange()
    {
        var optionToSet = GetDifferentOptionThanCurrentlySelected();

        var waitTask = _haContext.StateChanges()
            .Where(n => n.Entity.EntityId == "input_text.test_result")
            .Timeout(TimeSpan.FromMilliseconds(5000))
            .FirstAsync()
            .ToTask();

        _haContext.CallService(
            "input_select",
            "select_option",
            ServiceTarget.FromEntities("input_select.who_cooks"),
            new {option = optionToSet});

        var result = await waitTask.ConfigureAwait(false);

        result.New!.State.Should().Be(optionToSet);
    }

    private string GetDifferentOptionThanCurrentlySelected()
    {
        var currentState = _haContext.GetState("input_select.who_cooks")?.State
                            ?? throw new InvalidOperationException();

        var useOption = currentState switch
        {
            "Paulus" => "Anne Therese",
            _ => "Paulus"
        };
        return useOption;
    }
}
