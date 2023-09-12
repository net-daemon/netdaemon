using System.Net;
using System.Reactive.Subjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.AppModel;
using NetDaemon.Client.Internal.Exceptions;
using NetDaemon.HassModel;
using NetDaemon.Runtime.Internal;

namespace NetDaemon.Runtime.Tests.Internal;

public class AppStateManagerTests
{
    [Fact]
    public async Task TestGetStateAsyncReturnsCorrectStateEnabled()
    {
        // ARRANGE
        var haConnectionMock = new Mock<IHomeAssistantConnection>();
        var haRunnerMock = new Mock<IHomeAssistantRunner>();
        haRunnerMock.SetupGet(n => n.CurrentConnection).Returns(haConnectionMock.Object);

        var provider = new ServiceCollection()
            .AddSingleton(haRunnerMock.Object)
            .AddSingleton(new Mock<IHostEnvironment>().Object)
            .AddNetDaemonStateManager()
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
        var (haConnectionMock, provider) = SetupProviderAndMocks();

        var appStateManager = provider.GetRequiredService<IAppStateManager>();

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
        var (haConnectionMock, scopedProvider) = SetupProviderAndMocks();

        var appStateManager = scopedProvider.GetRequiredService<IAppStateManager>();
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
            n.SendCommandAsync(It.IsAny<CallServiceCommand>(),
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TestGetStateAsyncNotExistReturnsCorrectStateEnabled()
    {
        // ARRANGE
        var (haConnectionMock, provider) = SetupProviderAndMocks();

        var appStateManager = provider.GetRequiredService<IAppStateManager>();
        haConnectionMock.Setup(n => n.GetApiCallAsync<HassState>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new HomeAssistantApiCallException("ohh no", HttpStatusCode.NotFound));
        // ACT
        _ = await appStateManager.GetStateAsync("helloapp");
        // ASSERT
        haConnectionMock.Verify(n =>
            n.GetApiCallAsync<HassState>("states/input_boolean.netdaemon_helloapp", It.IsAny<CancellationToken>()));
        // It exists so it should turn it on
        haConnectionMock.Verify(n =>
            n.SendCommandAndReturnResponseAsync<CreateInputBooleanHelperCommand, InputBooleanHelper>(
                It.IsAny<CreateInputBooleanHelperCommand>(), It.IsAny<CancellationToken>()));
        haConnectionMock.Verify(n =>
            n.SendCommandAsync(It.IsAny<CallServiceCommand>(),
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TestGetStateAsyncNotExistReturnsCorrectStateEnabledInDevelopment()
    {
        // ARRANGE
        var (haConnectionMock, provider) = SetupProviderAndMocksDevelopment();

        var appStateManager = provider.GetRequiredService<IAppStateManager>();
        haConnectionMock.Setup(n => n.GetApiCallAsync<HassState>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new HomeAssistantApiCallException("ohh no", HttpStatusCode.NotFound));
        // ACT
        _ = await appStateManager.GetStateAsync("helloapp");
        // ASSERT
        haConnectionMock.Verify(n =>
            n.GetApiCallAsync<HassState>("states/input_boolean.dev_netdaemon_helloapp", It.IsAny<CancellationToken>()));
        // It exists so it should turn it on
        haConnectionMock.Verify(n =>
            n.SendCommandAndReturnResponseAsync<CreateInputBooleanHelperCommand, InputBooleanHelper>(
                It.IsAny<CreateInputBooleanHelperCommand>(), It.IsAny<CancellationToken>()));
        haConnectionMock.Verify(n =>
            n.SendCommandAsync(It.IsAny<CallServiceCommand>(),
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TestSetStateAsyncEnabled()
    {
        // ARRANGE
        var (haConnectionMock, provider) = SetupProviderAndMocks();

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
            n.SendCommandAsync(It.IsAny<CallServiceCommand>(),
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TestSetStateAsyncRunning()
    {
        // ARRANGE
        var (haConnectionMock, provider) = SetupProviderAndMocks();

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
        var (haConnectionMock, provider) = SetupProviderAndMocks();

        var appStateManager = provider.GetRequiredService<IAppStateManager>();

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
    public async Task TestInitialize()
    {
        // ARRANGE
        var haContextMock = new Mock<IHaContext>();
        var appModelContextMock = new Mock<IAppModelContext>();
        appModelContextMock.SetupGet(n => n.Applications)
            .Returns(new List<IApplication>() {new Mock<IApplication>().Object});

        var haConnectionMock = new Mock<IHomeAssistantConnection>();

        var haRunnerMock = new Mock<IHomeAssistantRunner>();
        haRunnerMock.SetupGet(n => n.CurrentConnection).Returns(haConnectionMock.Object);

        var repositoryMock = new Mock<IAppStateRepository>();

        var provider = new ServiceCollection()
            .AddSingleton(haRunnerMock.Object)
            .AddScoped(_ => haContextMock.Object)
            .AddSingleton(new Mock<IHostEnvironment>().Object)
            .AddNetDaemonStateManager()
            .AddSingleton(repositoryMock.Object)
            .BuildServiceProvider();
        using var scopedProvider = provider.CreateScope();

        var homeAssistantStateUpdater =
            scopedProvider.ServiceProvider.GetRequiredService<IHandleHomeAssistantAppStateUpdates>();
        Subject<HassEvent> hassEvent = new();
        haConnectionMock.Setup(n => n.SubscribeToHomeAssistantEventsAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(hassEvent);

        // ACT
        await homeAssistantStateUpdater.InitializeAsync(haConnectionMock.Object, appModelContextMock.Object)
            .ConfigureAwait(false);
        // ASSERT
        hassEvent.HasObservers.Should().BeTrue();
        repositoryMock.Verify(n => n.RemoveNotUsedStatesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InitializeShouldNeverTryDeleteUnusedInputBooleanHelpersWhenInDevelopmentEnvironment()
    {
        // ARRANGE
        var haContextMock = new Mock<IHaContext>();
        var appModelContextMock = new Mock<IAppModelContext>();
        // Make sure we have at least one application for this scenario
        appModelContextMock.SetupGet(n => n.Applications)
            .Returns(new List<IApplication>(){new Mock<IApplication>().Object});
        var haConnectionMock = new Mock<IHomeAssistantConnection>();
        
        haConnectionMock.Setup(n => n.SubscribeToHomeAssistantEventsAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Subject<HassEvent>());
        var haRunnerMock = new Mock<IHomeAssistantRunner>();
        
        haRunnerMock.SetupGet(n => n.CurrentConnection).Returns(haConnectionMock.Object);

        var environmentMock = new Mock<IHostEnvironment>();
        environmentMock.Setup(n => n.EnvironmentName).Returns("Development");
        var repositoryMock = new Mock<IAppStateRepository>();

        var provider = new ServiceCollection()
            .AddSingleton(haRunnerMock.Object)
            .AddScoped(_ => haContextMock.Object)
            .AddSingleton(environmentMock.Object)
            .AddNetDaemonStateManager()
            .AddSingleton(repositoryMock.Object)
            .BuildServiceProvider();
        using var scopedProvider = provider.CreateScope();

        var homeAssistantStateUpdater =
            scopedProvider.ServiceProvider.GetRequiredService<IHandleHomeAssistantAppStateUpdates>();

        // ACT
        await homeAssistantStateUpdater.InitializeAsync(haConnectionMock.Object, appModelContextMock.Object)
            .ConfigureAwait(false);
        // ASSERT
        repositoryMock.Verify(n => n.RemoveNotUsedStatesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Never);

    }

    [Fact]
    public async Task TestAppDisabledShouldCallSetStateAsyncEnabled()
    {
        // ARRANGE
        var (haConnectionMock, provider) = SetupProviderAndMocks();
        var appModelContextMock = new Mock<IAppModelContext>();
        appModelContextMock.SetupGet(n => n.Applications).Returns(new List<IApplication>());
        var appMock = new Mock<IApplication>();

        var homeAssistantStateUpdater =
            provider.GetRequiredService<IHandleHomeAssistantAppStateUpdates>();
        Subject<HassEvent> hassEvent = new();
        
        haConnectionMock.Setup(n => n.SubscribeToHomeAssistantEventsAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(hassEvent);
        
        appMock.SetupGet(n => n.Id).Returns("app");
        
        appModelContextMock.SetupGet(n => n.Applications).Returns(
            new List<IApplication>
            {
                appMock.Object
            });

        // ACT
        await homeAssistantStateUpdater.InitializeAsync(haConnectionMock.Object, appModelContextMock.Object)
            .ConfigureAwait(false);

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
        appMock.Verify(n => n.DisableAsync(), Times.Once);
    }

    [Fact]
    public async Task TestAppNoChangeShouldNotCallSetStateAsync()
    {
        // ARRANGE
        var (haConnectionMock, provider) = SetupProviderAndMocks();

        var homeAssistantStateUpdater =
            provider.GetRequiredService<IHandleHomeAssistantAppStateUpdates>();

        Subject<HassEvent> hassEvent = new();

        var appModelContextMock = new Mock<IAppModelContext>();
        appModelContextMock.SetupGet(n => n.Applications).Returns(new List<IApplication>());

        var appMock = new Mock<IApplication>();

        
        haConnectionMock.Setup(n => n.SubscribeToHomeAssistantEventsAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(hassEvent);
        
        appMock.SetupGet(n => n.Id).Returns("app");
        
        appModelContextMock.SetupGet(n => n.Applications).Returns(
            new List<IApplication>
            {
                appMock.Object
            });

        // ACT
        await homeAssistantStateUpdater.InitializeAsync(haConnectionMock.Object, appModelContextMock.Object)
            .ConfigureAwait(false);

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
        appMock.Verify(n => n.DisableAsync(), Times.Never);
    }

    [Fact]
    public async Task TestAppOneStateIsNullShouldNotCallSetStateAsync()
    {
        // ARRANGE
        var (haConnectionMock, provider) = SetupProviderAndMocks();

        var appModelContextMock = new Mock<IAppModelContext>();
        appModelContextMock.SetupGet(n => n.Applications).Returns(new List<IApplication>());

        var appMock = new Mock<IApplication>();

        var haRunnerMock = new Mock<IHomeAssistantRunner>();
        haRunnerMock.SetupGet(n => n.CurrentConnection).Returns(haConnectionMock.Object);


        var homeAssistantStateUpdater =
            provider.GetRequiredService<IHandleHomeAssistantAppStateUpdates>();
        
        Subject<HassEvent> hassEvent = new();
        
        haConnectionMock.Setup(n => n.SubscribeToHomeAssistantEventsAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(hassEvent);
        
        appMock.SetupGet(n => n.Id).Returns("app");
        
        appModelContextMock.SetupGet(n => n.Applications).Returns(
            new List<IApplication>
            {
                appMock.Object
            });

        // ACT
        await homeAssistantStateUpdater.InitializeAsync(haConnectionMock.Object, appModelContextMock.Object);
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
        appMock.Verify(n => n.DisableAsync(), Times.Never);
    }

    private (Mock<IHomeAssistantConnection> connection, IServiceProvider serviceProvider) SetupProviderAndMocks()
    {
        var haConnectionMock = new Mock<IHomeAssistantConnection>();
        var haRunnerMock = new Mock<IHomeAssistantRunner>();
        haRunnerMock.SetupGet(n => n.CurrentConnection).Returns(haConnectionMock.Object);

        var provider = new ServiceCollection()
            .AddSingleton(haRunnerMock.Object)
            .AddSingleton(new Mock<IHostEnvironment>().Object)
            .AddNetDaemonStateManager()
            .BuildServiceProvider();
        var scopedProvider = provider.CreateScope();

        return (haConnectionMock, scopedProvider.ServiceProvider);
    }

    private (Mock<IHomeAssistantConnection> connection, IServiceProvider serviceProvider) SetupProviderAndMocksDevelopment()
    {
        var haConnectionMock = new Mock<IHomeAssistantConnection>();
        var haRunnerMock = new Mock<IHomeAssistantRunner>();
        haRunnerMock.SetupGet(n => n.CurrentConnection).Returns(haConnectionMock.Object);
        var environmentMock = new Mock<IHostEnvironment>();
        environmentMock.Setup(n => n.EnvironmentName).Returns("Development");
        var provider = new ServiceCollection()
            .AddSingleton(haRunnerMock.Object)
            .AddSingleton(environmentMock.Object)
            .AddNetDaemonStateManager()
            .BuildServiceProvider();
        var scopedProvider = provider.CreateScope();

        return (haConnectionMock, scopedProvider.ServiceProvider);
    }
}
