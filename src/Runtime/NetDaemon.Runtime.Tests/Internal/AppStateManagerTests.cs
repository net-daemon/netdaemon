using Microsoft.Extensions.DependencyInjection;
using NetDaemon.AppModel;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.Runtime.Tests.Internal;

public class AppStateManagerTests
{
    [Fact]
    public async Task TestGetStateAsyncReturnsCorrectStateEnabled()
    {
        // ARRANGE
        var haContextMock = new Mock<IHaContext>();
        var provider = new ServiceCollection()
            .AddScoped(_ => haContextMock.Object)
            .AddNetDameonStateManager()
            .BuildServiceProvider();
        using var scopedProvider = provider.CreateScope();

        var appStateManager = scopedProvider.ServiceProvider.GetRequiredService<IAppStateManager>();

        // ACT
        // ASSERT
        haContextMock.Setup(n => n.GetState(It.IsAny<string>())).Returns(
            new EntityState
            {
                EntityId = "switch.helloapp",
                State = "on"
            }
        );
        (await appStateManager.GetStateAsync("hellpapp"))
            .Should().Be(ApplicationState.Enabled);
    }

    [Fact]
    public async Task TestGetStateAsyncReturnsCorrectStateDisabled()
    {
        // ARRANGE
        var haContextMock = new Mock<IHaContext>();
        var provider = new ServiceCollection()
            .AddScoped(_ => haContextMock.Object)
            .AddNetDameonStateManager()
            .BuildServiceProvider();
        using var scopedProvider = provider.CreateScope();

        var appStateManager = scopedProvider.ServiceProvider.GetRequiredService<IAppStateManager>();

        // ACT
        // ASSERT
        haContextMock.Setup(n => n.GetState(It.IsAny<string>())).Returns(
            new EntityState
            {
                EntityId = "switch.helloapp",
                State = "off"
            }
        );
        (await appStateManager.GetStateAsync("hellpapp"))
            .Should().Be(ApplicationState.Disabled);
    }

    [Fact]
    public async Task TestGetStateAsyncNotExistReturnsCorrectStateEnabled()
    {
        // ARRANGE
        var haContextMock = new Mock<IHaContext>();
        var provider = new ServiceCollection()
            .AddScoped(_ => haContextMock.Object)
            .AddNetDameonStateManager()
            .BuildServiceProvider();
        using var scopedProvider = provider.CreateScope();

        var appStateManager = scopedProvider.ServiceProvider.GetRequiredService<IAppStateManager>();

        // ACT
        // ASSERT
        haContextMock.Setup(n => n.GetState(It.IsAny<string>())).Returns(
            (EntityState?) null
        );
        (await appStateManager.GetStateAsync("hellpapp"))
            .Should().Be(ApplicationState.Enabled);

        haContextMock.Verify(n => n.CallService("netdaemon", "entity_create", null, It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public async Task TestSetStateAsyncEnabled()
    {
        // ARRANGE
        var haContextMock = new Mock<IHaContext>();
        var provider = new ServiceCollection()
            .AddScoped(_ => haContextMock.Object)
            .AddNetDameonStateManager()
            .BuildServiceProvider();
        using var scopedProvider = provider.CreateScope();

        var appStateManager = scopedProvider.ServiceProvider.GetRequiredService<IAppStateManager>();

        haContextMock.Setup(n => n.GetState(It.IsAny<string>())).Returns(
            new EntityState
            {
                EntityId = "switch.helloapp"
            });
        // ACT
        await appStateManager.SaveStateAsync("helloapp", ApplicationState.Enabled);
        // ASSERT

        haContextMock.Verify(n => n.CallService("netdaemon", "entity_update", null, It.IsAny<object?>()), Times.Once);
        var invocation = haContextMock.Invocations.First(n => n.Method.Name == "CallService").Arguments[3];
        invocation.GetType().GetProperty("entity_id")!.GetValue(invocation, null)!.Should()
            .Be("switch.netdaemon_helloapp");
        invocation.GetType().GetProperty("state")!.GetValue(invocation, null)!.Should().Be("on");
        var attributes = invocation.GetType().GetProperty("attributes")!.GetValue(invocation, null)!;
        attributes.GetType().GetProperty("app_state")!.GetValue(attributes, null).Should().Be("enabled");
    }

    [Fact]
    public async Task TestSetStateAsyncRunning()
    {
        // ARRANGE
        var haContextMock = new Mock<IHaContext>();
        var provider = new ServiceCollection()
            .AddScoped(_ => haContextMock.Object)
            .AddNetDameonStateManager()
            .BuildServiceProvider();
        using var scopedProvider = provider.CreateScope();

        var appStateManager = scopedProvider.ServiceProvider.GetRequiredService<IAppStateManager>();

        haContextMock.Setup(n => n.GetState(It.IsAny<string>())).Returns(
            new EntityState
            {
                EntityId = "switch.helloapp"
            });
        // ACT
        await appStateManager.SaveStateAsync("helloapp", ApplicationState.Running);
        // ASSERT

        haContextMock.Verify(n => n.CallService("netdaemon", "entity_update", null, It.IsAny<object?>()), Times.Once);
        var invocation = haContextMock.Invocations.First(n => n.Method.Name == "CallService").Arguments[3];
        invocation.GetType().GetProperty("entity_id")!.GetValue(invocation, null)!.Should()
            .Be("switch.netdaemon_helloapp");
        invocation.GetType().GetProperty("state")!.GetValue(invocation, null)!.Should().Be("on");
        var attributes = invocation.GetType().GetProperty("attributes")!.GetValue(invocation, null)!;
        attributes.GetType().GetProperty("app_state")!.GetValue(attributes, null).Should().Be("running");
    }

    [Fact]
    public async Task TestSetStateAsyncError()
    {
        // ARRANGE
        var haContextMock = new Mock<IHaContext>();
        var provider = new ServiceCollection()
            .AddScoped(_ => haContextMock.Object)
            .AddNetDameonStateManager()
            .BuildServiceProvider();
        using var scopedProvider = provider.CreateScope();

        var appStateManager = scopedProvider.ServiceProvider.GetRequiredService<IAppStateManager>();

        haContextMock.Setup(n => n.GetState(It.IsAny<string>())).Returns(
            new EntityState
            {
                EntityId = "switch.helloapp"
            });
        // ACT
        await appStateManager.SaveStateAsync("helloapp", ApplicationState.Error);
        // ASSERT

        haContextMock.Verify(n => n.CallService("netdaemon", "entity_update", null, It.IsAny<object?>()), Times.Once);
        var invocation = haContextMock.Invocations.First(n => n.Method.Name == "CallService").Arguments[3];
        invocation.GetType().GetProperty("entity_id")!.GetValue(invocation, null)!.Should()
            .Be("switch.netdaemon_helloapp");
        invocation.GetType().GetProperty("state")!.GetValue(invocation, null)!.Should().Be("on");
        var attributes = invocation.GetType().GetProperty("attributes")!.GetValue(invocation, null);
        attributes?.GetType().GetProperty("app_state")!.GetValue(attributes, null).Should().Be("error");
    }

    [Fact]
    public async Task TestSetStateAsyncDisabled()
    {
        // ARRANGE
        var haContextMock = new Mock<IHaContext>();
        var provider = new ServiceCollection()
            .AddScoped(_ => haContextMock.Object)
            .AddNetDameonStateManager()
            .BuildServiceProvider();
        using var scopedProvider = provider.CreateScope();

        var appStateManager = scopedProvider.ServiceProvider.GetRequiredService<IAppStateManager>();

        haContextMock.Setup(n => n.GetState(It.IsAny<string>())).Returns(
            new EntityState
            {
                EntityId = "switch.helloapp"
            });
        // ACT
        await appStateManager.SaveStateAsync("helloapp", ApplicationState.Disabled);
        // ASSERT

        haContextMock.Verify(n => n.CallService("netdaemon", "entity_update", null, It.IsAny<object?>()), Times.Once);
        var invocation = haContextMock.Invocations.First(n => n.Method.Name == "CallService").Arguments[3];
        invocation.GetType().GetProperty("entity_id")!.GetValue(invocation, null)!.Should()
            .Be("switch.netdaemon_helloapp");
        invocation.GetType().GetProperty("state")!.GetValue(invocation, null)!.Should().Be("off");
        var attributes = invocation.GetType().GetProperty("attributes")!.GetValue(invocation, null)!;
        attributes.GetType().GetProperty("app_state")!.GetValue(attributes, null).Should().Be("disabled");
    }
}