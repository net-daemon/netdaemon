using System.Net;
using System.Reactive.Subjects;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.AppModel;
using NetDaemon.Client.Internal.Exceptions;
using NetDaemon.HassModel.Common;
using NetDaemon.Runtime.Internal;
using NetDaemon.Runtime.Internal.Model;

namespace NetDaemon.Runtime.Tests.Internal;

public class AppStateManagerTests
{
    [Fact]
    public async Task TestGetStateAsyncReturnsCorrectStateEnabled()
    {
        // ARRANGE
        var haConnectionMock = new Mock<IHomeAssistantConnection>();
        var provider = new ServiceCollection()
            .AddTransient(_ => haConnectionMock.Object)
            .AddNetDameonStateManager()
            .BuildServiceProvider();
        using var scopedProvider = provider.CreateScope();

        var appStateManager = scopedProvider.ServiceProvider.GetRequiredService<IAppStateManager>();

        // ACT
        // ASSERT
        haConnectionMock.Setup(n => n.GetApiCallAsync<HassState>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new HassState
                {
                    EntityId = "input_boolean.helloapp",
                    State = "on"
                });
        (await appStateManager.GetStateAsync("hellpapp"))
            .Should().Be(ApplicationState.Enabled);
    }

    [Fact]
    public async Task TestGetStateAsyncReturnsCorrectStateDisabled()
    {
        // ARRANGE
        var haConnectionMock = new Mock<IHomeAssistantConnection>();
        var provider = new ServiceCollection()
            .AddTransient(_ => haConnectionMock.Object)
            .AddNetDameonStateManager()
            .BuildServiceProvider();
        using var scopedProvider = provider.CreateScope();

        var appStateManager = scopedProvider.ServiceProvider.GetRequiredService<IAppStateManager>();

        // ACT
        // ASSERT
        haConnectionMock.Setup(n => n.GetApiCallAsync<HassState>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new HassState
                {
                    EntityId = "input_boolean.helloapp",
                    State = "off"
                });
        (await appStateManager.GetStateAsync("hellpapp"))
            .Should().Be(ApplicationState.Disabled);
    }

    [Fact]
    public async Task TestSaveStateAsyncReturnsCorrectStateDisabled()
    {
        // ARRANGE
        var haConnectionMock = new Mock<IHomeAssistantConnection>();
        var provider = new ServiceCollection()
            .AddTransient(_ => haConnectionMock.Object)
            .AddNetDameonStateManager()
            .BuildServiceProvider();

        var appStateManager = provider.GetRequiredService<IAppStateManager>();
        haConnectionMock.Setup(n => n.GetApiCallAsync<HassState>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new HassState
                {
                    EntityId = "input_boolean.helloapp",
                    State = "on"
                });
        // ACT
        await appStateManager.SaveStateAsync("helloapp", ApplicationState.Disabled);

        // ASSERT
        haConnectionMock.Verify(n =>
            n.GetApiCallAsync<HassState>("states/input_boolean.netdaemon_helloapp", It.IsAny<CancellationToken>()));
        // It exists so it should turn it on
        haConnectionMock.Verify(n =>
            n.SendCommandAndReturnResponseAsync<CallServiceCommand, object>(It.IsAny<CallServiceCommand>(),
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TestGetStateAsyncNotExistReturnsCorrectStateEnabled()
    {
        // ARRANGE
        var haConnectionMock = new Mock<IHomeAssistantConnection>();
        var provider = new ServiceCollection()
            .AddTransient(_ => haConnectionMock.Object)
            .AddNetDameonStateManager()
            .BuildServiceProvider();

        var appStateManager = provider.GetRequiredService<IAppStateManager>();
        haConnectionMock.Setup(n => n.GetApiCallAsync<HassState>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new HomeAssistantApiCallException("ohh no", HttpStatusCode.NotFound));
        // ACT
        var state = await appStateManager.GetStateAsync("helloapp");
        // ASSERT
        haConnectionMock.Verify(n =>
            n.GetApiCallAsync<HassState>("states/input_boolean.netdaemon_helloapp", It.IsAny<CancellationToken>()));
        // It exists so it should turn it on
        haConnectionMock.Verify(n =>
            n.SendCommandAndReturnResponseAsync<CreateInputBooleanHelperCommand, InputBooleanHelper>(
                It.IsAny<CreateInputBooleanHelperCommand>(), It.IsAny<CancellationToken>()));
        haConnectionMock.Verify(n =>
            n.SendCommandAndReturnResponseAsync<CallServiceCommand, object>(It.IsAny<CallServiceCommand>(),
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TestSetStateAsyncEnabled()
    {
        // ARRANGE
        var haConnectionMock = new Mock<IHomeAssistantConnection>();
        var provider = new ServiceCollection()
            .AddTransient(_ => haConnectionMock.Object)
            .AddNetDameonStateManager()
            .BuildServiceProvider();

        var appStateManager = provider.GetRequiredService<IAppStateManager>();
        haConnectionMock.Setup(n => n.GetApiCallAsync<HassState>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new HassState
                {
                    EntityId = "input_boolean.helloapp",
                    State = "off"
                });
        // ACT
        await appStateManager.SaveStateAsync("helloapp", ApplicationState.Enabled);

        // ASSERT
        haConnectionMock.Verify(n =>
            n.GetApiCallAsync<HassState>("states/input_boolean.netdaemon_helloapp", It.IsAny<CancellationToken>()));
        // It exists so it should turn it on
        haConnectionMock.Verify(n =>
            n.SendCommandAndReturnResponseAsync<CallServiceCommand, object>(It.IsAny<CallServiceCommand>(),
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TestSetStateAsyncRunning()
    {
        // ARRANGE
        var haConnectionMock = new Mock<IHomeAssistantConnection>();
        var provider = new ServiceCollection()
            .AddTransient(_ => haConnectionMock.Object)
            .AddNetDameonStateManager()
            .BuildServiceProvider();

        var appStateManager = provider.GetRequiredService<IAppStateManager>();
        haConnectionMock.Setup(n => n.GetApiCallAsync<HassState>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new HassState
                {
                    EntityId = "input_boolean.helloapp",
                    State = "on"
                });
        // ACT
        await appStateManager.SaveStateAsync("helloapp", ApplicationState.Running);

        // ASSERT
        // This should just render in a get api check
        haConnectionMock.Verify(n =>
            n.GetApiCallAsync<HassState>("states/input_boolean.netdaemon_helloapp", It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TestSetStateAsyncError()
    {
        // ARRANGE
        var haConnectionMock = new Mock<IHomeAssistantConnection>();
        var provider = new ServiceCollection()
            .AddTransient(_ => haConnectionMock.Object)
            .AddNetDameonStateManager()
            .BuildServiceProvider();
        using var scopedProvider = provider.CreateScope();

        var appStateManager = scopedProvider.ServiceProvider.GetRequiredService<IAppStateManager>();

        haConnectionMock.Setup(n => n.GetApiCallAsync<HassState>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new HassState
                {
                    EntityId = "input_boolean.helloapp",
                    State = "on"
                });
        // ACT
        await appStateManager.SaveStateAsync("helloapp", ApplicationState.Error);
        // This should just render in a get api check
        // ASSERT
        haConnectionMock.Verify(n =>
            n.GetApiCallAsync<HassState>("states/input_boolean.netdaemon_helloapp", It.IsAny<CancellationToken>()));
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

        var homeAssistantStateUpdater =
            scopedProvider.ServiceProvider.GetRequiredService<IHandleHomeAssistantAppStateUpdates>();
        Subject<HassEvent> hassEvent = new();
        haConnectionMock.SetupGet(n => n.OnHomeAssistantEvent).Returns(hassEvent);

        // ACT
        homeAssistantStateUpdater.Initialize(haConnectionMock.Object, appModelContextMock.Object);
        // ASSERT
        hassEvent.HasObservers.Should().BeTrue();
    }

    // [Fact]
    // public void TestAppEnabledShouldCallSetStateAsyncDisabled()
    // {
    //     // ARRANGE
    //     var haContextMock = new Mock<IHaContext>();
    //     var appModelContextMock = new Mock<IAppModelContext>();
    //     var appMock = new Mock<IApplication>();
    //     var haConnectionMock = new Mock<IHomeAssistantConnection>();
    //     var provider = new ServiceCollection()
    //         .AddScoped(_ => haContextMock.Object)
    //         .AddTransient(_ => haConnectionMock.Object)
    //         .AddNetDameonStateManager()
    //         .BuildServiceProvider();
    //     using var scopedProvider = provider.CreateScope();
    //
    //     var homeAssistantStateUpdater =
    //         scopedProvider.ServiceProvider.GetRequiredService<IHandleHomeAssistantAppStateUpdates>();
    //     Subject<HassEvent> hassEvent = new();
    //     haConnectionMock.SetupGet(n => n.OnHomeAssistantEvent).Returns(hassEvent);
    //     appMock.SetupGet(n => n.Id).Returns("app");
    //     appModelContextMock.SetupGet(n => n.Applications).Returns(
    //         new List<IApplication>
    //         {
    //             appMock.Object
    //         });
    //
    //     // ACT
    //     homeAssistantStateUpdater.Initialize(haConnectionMock.Object, appModelContextMock.Object);
    //     hassEvent.OnNext(new HassEvent
    //     {
    //         EventType = "state_changed",
    //         DataElement = new HassStateChangedEventData
    //         {
    //             EntityId = "switch.netdaemon_app",
    //             NewState = new HassState
    //             {
    //                 EntityId = "switch.netdaemon_app",
    //                 State = "on"
    //             },
    //             OldState = new HassState
    //             {
    //                 EntityId = "switch.netdaemon_app",
    //                 State = "off"
    //             }
    //         }.ToJsonElement()
    //     });
    //     // ASSERT
    //     appMock.Verify(n => n.SetStateAsync(ApplicationState.Enabled), Times.Once);
    // }

    [Fact]
    public void TestAppDisabledShouldCallSetStateAsyncEnabled()
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

        var homeAssistantStateUpdater =
            scopedProvider.ServiceProvider.GetRequiredService<IHandleHomeAssistantAppStateUpdates>();
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
        hassEvent.OnNext(new HassEvent
        {
            EventType = "state_changed",
            DataElement = new HassStateChangedEventData
            {
                EntityId = "input_boolean.netdaemon_app",
                NewState = new HassState
                {
                    EntityId = "input_boolean.netdaemon_app",
                    State = "off"
                },
                OldState = new HassState
                {
                    EntityId = "input_boolean.netdaemon_app",
                    State = "on"
                }
            }.ToJsonElement()
        });
        // ASSERT
        appMock.Verify(n => n.SetStateAsync(ApplicationState.Disabled), Times.Once);
    }

    [Fact]
    public void TestAppNoChangeShouldNotCallSetStateAsync()
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

        var homeAssistantStateUpdater =
            scopedProvider.ServiceProvider.GetRequiredService<IHandleHomeAssistantAppStateUpdates>();
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
        hassEvent.OnNext(new HassEvent
        {
            EventType = "state_changed",
            DataElement = new HassStateChangedEventData
            {
                EntityId = "input_boolean.netdaemon_app",
                NewState = new HassState
                {
                    EntityId = "input_boolean.netdaemon_app",
                    State = "on"
                },
                OldState = new HassState
                {
                    EntityId = "input_boolean.netdaemon_app",
                    State = "on"
                }
            }.ToJsonElement()
        });

        // ASSERT
        appMock.Verify(n => n.SetStateAsync(ApplicationState.Disabled), Times.Never);
    }

    [Fact]
    public void TestAppOneStateIsNullShouldNotCallSetStateAsync()
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

        var homeAssistantStateUpdater =
            scopedProvider.ServiceProvider.GetRequiredService<IHandleHomeAssistantAppStateUpdates>();
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
        hassEvent.OnNext(new HassEvent
        {
            EventType = "state_changed",
            DataElement = new HassStateChangedEventData
            {
                EntityId = "input_boolean.netdaemon_app",
                NewState = new HassState
                {
                    EntityId = "input_boolean.netdaemon_app",
                    State = "on"
                }
            }.ToJsonElement()
        });

        // ASSERT
        appMock.Verify(n => n.SetStateAsync(ApplicationState.Disabled), Times.Never);
    }
}