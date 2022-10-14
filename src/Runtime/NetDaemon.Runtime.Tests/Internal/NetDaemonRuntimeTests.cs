using System.Reactive.Subjects;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.AppModel;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Internal;
using NetDaemon.Runtime.Internal;

namespace NetDaemon.Runtime.Tests.Internal;

public class NetDaemonRuntimeTests
{
    [Fact]
    public async Task TestExecuteAsync()
    {
        var homeAssistantRunnerMock = new Mock<IHomeAssistantRunner>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<NetDaemonRuntime>>();

        var connectSubject = new Subject<IHomeAssistantConnection>();
        var disconnectSubject = new Subject<DisconnectReason>();
        homeAssistantRunnerMock.SetupGet(n => n.OnConnect).Returns(connectSubject);
        homeAssistantRunnerMock.SetupGet(n => n.OnDisconnect).Returns(disconnectSubject);
        var runtime = new NetDaemonRuntime(
            homeAssistantRunnerMock.Object,
            new FakeHassSettingsOptions(),
            new FakeApplicationLocationSettingsOptions(),
            serviceProviderMock.Object,
            loggerMock.Object,
            Mock.Of<ICacheManager>()
        );
        var cancelSource = new CancellationTokenSource(5000);
        var startingTask = runtime.StartAsync(cancelSource.Token);

        connectSubject.HasObservers.Should().BeTrue();
        disconnectSubject.HasObservers.Should().BeTrue();

        startingTask.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task TestOnConnect()
    {
        var cancelSource = new CancellationTokenSource(5000);
        var homeAssistantRunnerMock = new Mock<IHomeAssistantRunner>();
        var homeAssistantConnectionMock = new Mock<IHomeAssistantConnection>();
        var appModelMock = new Mock<IAppModel>();
        var loggerMock = new Mock<ILogger<NetDaemonRuntime>>();
        var scopedContext = new Mock<AppScopedHaContextProvider>();

        var connectSubject = new Subject<IHomeAssistantConnection>();
        var disconnectSubject = new Subject<DisconnectReason>();
        homeAssistantRunnerMock.SetupGet(n => n.OnConnect).Returns(connectSubject);
        homeAssistantRunnerMock.SetupGet(n => n.OnDisconnect).Returns(disconnectSubject);
        homeAssistantRunnerMock.Setup(n => n.RunAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns(async () => { await Task.Delay(-1, cancelSource.Token); });
        homeAssistantRunnerMock.SetupGet(n => n.CurrentConnection)
            .Returns(homeAssistantConnectionMock.Object);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScopedHaContext();
        var hassEventSubject = new Subject<HassEvent>();
        serviceCollection.AddTransient<IObservable<HassEvent>>(_ => hassEventSubject);
        serviceCollection.AddSingleton(_ => homeAssistantRunnerMock.Object);
        serviceCollection.AddScoped(_ => scopedContext);
        serviceCollection.AddSingleton(appModelMock.Object);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        await using var runtime = new NetDaemonRuntime(
            homeAssistantRunnerMock.Object,
            new FakeHassSettingsOptions(),
            new FakeApplicationLocationSettingsOptions(),
            serviceProvider,
            loggerMock.Object,
            Mock.Of<ICacheManager>()
        );
        var startingTask = runtime.StartAsync(cancelSource.Token);

        startingTask.IsCompleted.Should().BeFalse();
        connectSubject.OnNext(
            homeAssistantConnectionMock.Object
        );
        await startingTask.ConfigureAwait(false);


        appModelMock.Verify(n => n.LoadNewApplicationContext(It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TestOnDisconnect()
    {
        var cancelSource = new CancellationTokenSource(5000);
        var homeAssistantRunnerMock = new Mock<IHomeAssistantRunner>();
        var homeAssistantConnectionMock = new Mock<IHomeAssistantConnection>();
        var appModelMock = new Mock<IAppModel>();
        var loggerMock = new Mock<ILogger<NetDaemonRuntime>>();
        var scopedContext = new Mock<AppScopedHaContextProvider>();

        var connectSubject = new Subject<IHomeAssistantConnection>();
        var disconnectSubject = new Subject<DisconnectReason>();
        homeAssistantRunnerMock.SetupGet(n => n.OnConnect).Returns(connectSubject);
        homeAssistantRunnerMock.SetupGet(n => n.OnDisconnect).Returns(disconnectSubject);
        homeAssistantRunnerMock.Setup(n => n.RunAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns(async () => { await Task.Delay(-1, cancelSource.Token); });
        homeAssistantRunnerMock.SetupGet(n => n.CurrentConnection)
            .Returns(homeAssistantConnectionMock.Object);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScopedHaContext();
        var hassEventSubject = new Subject<HassEvent>();
        serviceCollection.AddTransient<IObservable<HassEvent>>(_ => hassEventSubject);
        serviceCollection.AddSingleton(_ => homeAssistantRunnerMock.Object);
        serviceCollection.AddScoped(_ => scopedContext);
        serviceCollection.AddSingleton(appModelMock.Object);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        await using var runtime = new NetDaemonRuntime(
            homeAssistantRunnerMock.Object,
            new FakeHassSettingsOptions(),
            new FakeApplicationLocationSettingsOptions(),
            serviceProvider,
            loggerMock.Object,
            Mock.Of<ICacheManager>()
        );
        var startingTask = runtime.StartAsync(cancelSource.Token);

        // First make sure we add an connection
        connectSubject.OnNext(
            homeAssistantConnectionMock.Object
        );

        await startingTask.ConfigureAwait(false);
        runtime.IsConnected.Should().BeTrue();

        // Then fake a disconnect
        disconnectSubject.OnNext(
            DisconnectReason.Client
        );

        runtime.IsConnected.Should().BeFalse();
    }

    private class FakeHassSettingsOptions : IOptions<HomeAssistantSettings>
    {
        public FakeHassSettingsOptions()
        {
            Value = new HomeAssistantSettings();
        }

        public HomeAssistantSettings Value { get; }
    }    
    
    private class FakeApplicationLocationSettingsOptions : IOptions<AppConfigurationLocationSetting>
    {
        public FakeApplicationLocationSettingsOptions()
        {
            Value = new AppConfigurationLocationSetting {ApplicationConfigurationFolder = "/test"};
        }

        public AppConfigurationLocationSetting Value { get; }
    }
}