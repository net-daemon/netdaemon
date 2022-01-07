using System.Reactive.Subjects;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.AppModel;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;
using NetDaemon.Runtime.Internal;
using NetDaemon.Runtime.Tests.Helpers;

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
    
    [Fact]
    public void TestInitialize()
    {
        // ARRANGE
        var haContextMock = new Mock<IHaContext>();
        var appModelContextMock = new Mock<IAppModelContext>();
        var haConnectionMock = new Mock<IHomeAssistantConnection>();
        var provider = new ServiceCollection()
            .AddScoped(_ => haContextMock.Object)
            .AddTransient(_ => haConnectionMock.Object)
            .AddNetDameonStateManager()
            .BuildServiceProvider();
        using var scopedProvider = provider.CreateScope();

        var homeAssistantStateUpdater = scopedProvider.ServiceProvider.GetRequiredService<IHandleHomeAssistantAppStateUpdates>();
        Subject<HassEvent> hassEvent = new();
        haConnectionMock.SetupGet(n => n.OnHomeAssistantEvent).Returns(hassEvent);
        
        // ACT
        homeAssistantStateUpdater.Initialize(haConnectionMock.Object, appModelContextMock.Object);
        // ASSERT
        hassEvent.HasObservers.Should().BeTrue();
    }
    
    [Fact]
    public async Task TestAppEnabledShouldCallSetStateAsyncDisabled()
    {
        // ARRANGE
        var haContextMock = new Mock<IHaContext>();
        var appModelContextMock = new Mock<IAppModelContext>();
        var appMock = new Mock<IApplication>();
        var haConnectionMock = new Mock<IHomeAssistantConnection>();
        var provider = new ServiceCollection()
            .AddScoped(_ => haContextMock.Object)
            .AddTransient(_ => haConnectionMock.Object)
            .AddNetDameonStateManager()
            .BuildServiceProvider();
        using var scopedProvider = provider.CreateScope();

        var homeAssistantStateUpdater = scopedProvider.ServiceProvider.GetRequiredService<IHandleHomeAssistantAppStateUpdates>();
        Subject<HassEvent> hassEvent = new();
        haConnectionMock.SetupGet(n => n.OnHomeAssistantEvent).Returns(hassEvent);
        appMock.SetupGet(n => n.Id).Returns("app");
        appModelContextMock.SetupGet(n => n.Applications).Returns(
            new List<IApplication>
            {
                appMock.Object
            });
        
        // ACT
        homeAssistantStateUpdater.Initialize(haConnectionMock.Object, appModelContextMock.Object);
        var invocationTask = appMock.WaitForInvocation(n => n.SetStateAsync(It.IsAny<ApplicationState>()));
        hassEvent.OnNext(new HassEvent()
        {
            EventType = "state_changed",
            DataElement = new HassStateChangedEventData
            {
                EntityId= "switch.netdaemon_app",
                NewState = new HassState
                {
                    EntityId = "switch.netdaemon_app",
                    State = "on"
                },
                OldState = new HassState
                {
                    EntityId = "switch.netdaemon_app",
                    State = "off"
                }
            }.ToJsonElement()
        });
        await invocationTask.ConfigureAwait(false);
        // ASSERT
        appMock.Verify(n => n.SetStateAsync(ApplicationState.Enabled), Times.Once);
    }
    
    [Fact]
    public async Task TestAppDisabledShouldCallSetStateAsyncEnabled()
    {
        // ARRANGE
        var haContextMock = new Mock<IHaContext>();
        var appModelContextMock = new Mock<IAppModelContext>();
        var appMock = new Mock<IApplication>();
        var haConnectionMock = new Mock<IHomeAssistantConnection>();
        var provider = new ServiceCollection()
            .AddScoped(_ => haContextMock.Object)
            .AddTransient(_ => haConnectionMock.Object)
            .AddNetDameonStateManager()
            .BuildServiceProvider();
        using var scopedProvider = provider.CreateScope();

        var homeAssistantStateUpdater = scopedProvider.ServiceProvider.GetRequiredService<IHandleHomeAssistantAppStateUpdates>();
        Subject<HassEvent> hassEvent = new();
        haConnectionMock.SetupGet(n => n.OnHomeAssistantEvent).Returns(hassEvent);
        appMock.SetupGet(n => n.Id).Returns("app");
        appModelContextMock.SetupGet(n => n.Applications).Returns(
            new List<IApplication>
            {
                appMock.Object
            });
        
        // ACT
        homeAssistantStateUpdater.Initialize(haConnectionMock.Object, appModelContextMock.Object);
        var invocationTask = appMock.WaitForInvocation(n => n.SetStateAsync(It.IsAny<ApplicationState>()));
        hassEvent.OnNext(new HassEvent()
        {
            EventType = "state_changed",
            DataElement = new HassStateChangedEventData
            {
                EntityId= "switch.netdaemon_app",
                NewState = new HassState
                {
                    EntityId = "switch.netdaemon_app",
                    State = "off"
                },
                OldState = new HassState
                {
                    EntityId = "switch.netdaemon_app",
                    State = "on"
                }
            }.ToJsonElement()
        });
        await invocationTask.ConfigureAwait(false);
        // ASSERT
        appMock.Verify(n => n.SetStateAsync(ApplicationState.Disabled), Times.Once);
    }    
    
    [Fact]
    public async Task TestAppNoChangeShouldNotCallSetStateAsync()
    {
        // ARRANGE
        var haContextMock = new Mock<IHaContext>();
        var appModelContextMock = new Mock<IAppModelContext>();
        var appMock = new Mock<IApplication>();
        var haConnectionMock = new Mock<IHomeAssistantConnection>();
        var provider = new ServiceCollection()
            .AddScoped(_ => haContextMock.Object)
            .AddTransient(_ => haConnectionMock.Object)
            .AddNetDameonStateManager()
            .BuildServiceProvider();
        using var scopedProvider = provider.CreateScope();

        var homeAssistantStateUpdater = scopedProvider.ServiceProvider.GetRequiredService<IHandleHomeAssistantAppStateUpdates>();
        Subject<HassEvent> hassEvent = new();
        haConnectionMock.SetupGet(n => n.OnHomeAssistantEvent).Returns(hassEvent);
        appMock.SetupGet(n => n.Id).Returns("app");
        appModelContextMock.SetupGet(n => n.Applications).Returns(
            new List<IApplication>
            {
                appMock.Object
            });
        
        // ACT
        homeAssistantStateUpdater.Initialize(haConnectionMock.Object, appModelContextMock.Object);
        var invocationTask = appMock.WaitForInvocation(n => n.SetStateAsync(It.IsAny<ApplicationState>()));
        hassEvent.OnNext(new HassEvent()
        {
            EventType = "state_changed",
            DataElement = new HassStateChangedEventData
            {
                EntityId= "switch.netdaemon_app",
                NewState = new HassState
                {
                    EntityId = "switch.netdaemon_app",
                    State = "on"
                },
                OldState = new HassState
                {
                    EntityId = "switch.netdaemon_app",
                    State = "on"
                }
            }.ToJsonElement()
        });
        try
        {
            await invocationTask.ConfigureAwait(false);
        }
        catch (TimeoutException )
        {
            // Ignore
        }
        // ASSERT
        appMock.Verify(n => n.SetStateAsync(ApplicationState.Disabled), Times.Never);
    }    
    [Fact]
    public async Task TestAppOneStateIsNullShouldNotCallSetStateAsync()
    {
        // ARRANGE
        var haContextMock = new Mock<IHaContext>();
        var appModelContextMock = new Mock<IAppModelContext>();
        var appMock = new Mock<IApplication>();
        var haConnectionMock = new Mock<IHomeAssistantConnection>();
        var provider = new ServiceCollection()
            .AddScoped(_ => haContextMock.Object)
            .AddTransient(_ => haConnectionMock.Object)
            .AddNetDameonStateManager()
            .BuildServiceProvider();
        using var scopedProvider = provider.CreateScope();

        var homeAssistantStateUpdater = scopedProvider.ServiceProvider.GetRequiredService<IHandleHomeAssistantAppStateUpdates>();
        Subject<HassEvent> hassEvent = new();
        haConnectionMock.SetupGet(n => n.OnHomeAssistantEvent).Returns(hassEvent);
        appMock.SetupGet(n => n.Id).Returns("app");
        appModelContextMock.SetupGet(n => n.Applications).Returns(
            new List<IApplication>
            {
                appMock.Object
            });
        
        // ACT
        homeAssistantStateUpdater.Initialize(haConnectionMock.Object, appModelContextMock.Object);
        var invocationTask = appMock.WaitForInvocation(n => n.SetStateAsync(It.IsAny<ApplicationState>()));
        hassEvent.OnNext(new HassEvent()
        {
            EventType = "state_changed",
            DataElement = new HassStateChangedEventData
            {
                EntityId= "switch.netdaemon_app",
                NewState = new HassState
                {
                    EntityId = "switch.netdaemon_app",
                    State = "on"
                }
            }.ToJsonElement()
        });
        try
        {
            await invocationTask.ConfigureAwait(false);
        }
        catch (TimeoutException )
        {
            // Ignore
        }
        // ASSERT
        appMock.Verify(n => n.SetStateAsync(ApplicationState.Disabled), Times.Never);
    }
}