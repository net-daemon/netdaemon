using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using FluentAssertions;
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
        using var netDaemon = StartNetDaemon();
        await using var scope = netDaemon.Services.CreateAsyncScope();
        
        var haContext = scope.ServiceProvider.GetRequiredService<IHaContext>();
        var optionToSet = GetDifferentOptionThanCurrentlySelected(haContext);
        
        var waitTask = haContext.StateChanges()
            .Where(n => n.Entity.EntityId == "input_text.test_result")
            .Timeout(TimeSpan.FromMilliseconds(5000))
            .FirstAsync()
            .ToTask();

        haContext.StateChanges()
            .Where(n => n.Entity.EntityId == "input_text.test_result").Subscribe(x =>
            {

            });
        
        haContext.CallService(
            "input_select",
            "select_option",
            ServiceTarget.FromEntities("input_select.who_cooks"),
            new {option = optionToSet});

        var result = await waitTask.ConfigureAwait(false);

        result.New!.State.Should().Be(optionToSet);
    }

    private string GetDifferentOptionThanCurrentlySelected(IHaContext haContext)
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