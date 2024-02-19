using System.Reactive.Subjects;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.AppModel;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Internal;
using NetDaemon.Runtime.Internal;

namespace NetDaemon.Runtime.Tests.Internal;

public sealed class NetDaemonRuntimeTests : IDisposable
{
    private Mock<IHomeAssistantRunner> _homeAssistantRunnerMock = new();
    private Mock<IHomeAssistantConnection> _homeAssistantConnectionMock = new();
    private Mock<IAppModel> _appModelMock = new();
    private Mock<ICacheManager> _cacheManagerMock = new();
    private readonly Mock<ILogger<NetDaemonRuntime>> _loggerMock = new();
    private Mock<AppScopedHaContextProvider> _scopedContext = new();
    private readonly Subject<IHomeAssistantConnection> _connectSubject = new();
    private readonly Subject<DisconnectReason> _disconnectSubject = new();


    [Fact]
    public async Task TestStartAsyncAsync()
    {
        using CancellationTokenSource fakeRunnerCancellationSource = new();
        await using var runtime = SetupNetDaemonRuntime(fakeRunnerCancellationSource.Token);
        var startingTask = runtime.StartAsync(CancellationToken.None);

        _connectSubject.HasObservers.Should().BeTrue();
        _disconnectSubject.HasObservers.Should().BeTrue();

        startingTask.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task TestOnConnect()
    {
        using CancellationTokenSource fakeRunnerCancellationSource = new();
        await using var runtime = SetupNetDaemonRuntime(fakeRunnerCancellationSource.Token);
        using var startingTask = runtime.StartAsync(CancellationToken.None);

        startingTask.IsCompleted.Should().BeFalse();
        _connectSubject.OnNext(_homeAssistantConnectionMock.Object);
        await startingTask.ConfigureAwait(false);

        _appModelMock.Verify(n => n.LoadNewApplicationContext(It.IsAny<CancellationToken>()));

        VerifyNoErrorsLogged();
    }

    [Fact]
    public async Task TestOnDisconnect()
    {
        using CancellationTokenSource fakeRunnerCancellationSource = new();
        await using var runtime = SetupNetDaemonRuntime(fakeRunnerCancellationSource.Token);
        var startingTask = runtime.StartAsync(CancellationToken.None);

        // First make sure we add an connection
        _connectSubject.OnNext(_homeAssistantConnectionMock.Object);

        await startingTask.ConfigureAwait(false);
        runtime.IsConnected.Should().BeTrue();

        // Then fake a disconnect
        _disconnectSubject.OnNext(DisconnectReason.Client);

        runtime.IsConnected.Should().BeFalse();

        VerifyNoErrorsLogged();
    }

    [Fact]
    public async Task TestReconnect()
    {
        using CancellationTokenSource fakeRunnerCancellationSource = new();
        await using var runtime = SetupNetDaemonRuntime(fakeRunnerCancellationSource.Token);
        var startingTask = runtime.StartAsync(CancellationToken.None);

        // First make sure we add an connection
        _connectSubject.OnNext(_homeAssistantConnectionMock.Object);

        await startingTask.ConfigureAwait(false);
        runtime.IsConnected.Should().BeTrue();

        // Then fake a disconnect
        _disconnectSubject.OnNext(DisconnectReason.Client);
        runtime.IsConnected.Should().BeFalse();

        // make a new connection
        _connectSubject.OnNext(_homeAssistantConnectionMock.Object);
        runtime.IsConnected.Should().BeTrue();

        // disconnect again
        _disconnectSubject.OnNext(DisconnectReason.Client);

        runtime.IsConnected.Should().BeFalse();

        VerifyNoErrorsLogged();
    }

    [Fact]
    public async Task TestOnConnectError()
    {
        _cacheManagerMock.Setup(m => m.InitializeAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Something wrong while initializing"));

        // We need to make sure we cancel the runtime after 3 seconds so we don't wait forever
        using CancellationTokenSource fakeRunnerCancellationSource = new(3000);
        await using var runtime = SetupNetDaemonRuntime(fakeRunnerCancellationSource.Token);

        var startAsync = runtime.StartAsync(CancellationToken.None);
        _connectSubject.OnNext(_homeAssistantConnectionMock.Object);

        await startAsync;
        runtime.IsConnected.Should().BeFalse();
    }


    [Fact]
    public async Task TestOnReConnectError()
    {
        using CancellationTokenSource fakeRunnerCancellationSource = new();
        await using var runtime = SetupNetDaemonRuntime(fakeRunnerCancellationSource.Token);

        var startAsync = runtime.StartAsync(CancellationToken.None);
        _connectSubject.OnNext(_homeAssistantConnectionMock.Object);
        _disconnectSubject.OnNext(DisconnectReason.Client);

        // now it should err on the second connection
        _cacheManagerMock.Setup(m => m.InitializeAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Something wrong while initializing"));
        _connectSubject.OnNext(_homeAssistantConnectionMock.Object);


        _loggerMock.Verify(
            x => x.Log(LogLevel.Critical,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), times: Times.Once);
    }

    private NetDaemonRuntime SetupNetDaemonRuntime(CancellationToken tokenSource)
    {
        _homeAssistantRunnerMock.SetupGet(n => n.CurrentConnection).Returns(_homeAssistantConnectionMock.Object);
        _homeAssistantRunnerMock.SetupGet(n => n.OnConnect).Returns(_connectSubject);
        _homeAssistantRunnerMock.SetupGet(n => n.OnDisconnect).Returns(_disconnectSubject);
        _homeAssistantRunnerMock.Setup(n => n.RunAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                // We set a 5 seconds delay instead of infinite in case we have a bad test
                return Task.Delay(5000, tokenSource);
            });

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScopedHaContext();
        var hassEventSubject = new Subject<HassEvent>();
        serviceCollection.AddTransient<IObservable<HassEvent>>(_ => hassEventSubject);
        serviceCollection.AddSingleton(_ => _homeAssistantRunnerMock.Object);
        serviceCollection.AddScoped(_ => _scopedContext);
        serviceCollection.AddSingleton(_appModelMock.Object);
        serviceCollection.AddSingleton<IOptions<HomeAssistantSettings>, FakeHassSettingsOptions>();
        serviceCollection.AddSingleton<IOptions<AppConfigurationLocationSetting>, FakeApplicationLocationSettingsOptions>();
        serviceCollection.AddSingleton(_cacheManagerMock.Object);
        serviceCollection.AddSingleton(_loggerMock.Object);
        serviceCollection.AddTransient<NetDaemonRuntime>();
        return serviceCollection.BuildServiceProvider().GetRequiredService<NetDaemonRuntime>();
    }

    private void VerifyNoErrorsLogged()
    {
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l >= LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), times: Times.Never);
    }

    private class FakeHassSettingsOptions : IOptions<HomeAssistantSettings>
    {
        public HomeAssistantSettings Value { get; } = new();
    }

    private class FakeApplicationLocationSettingsOptions : IOptions<AppConfigurationLocationSetting>
    {
        public AppConfigurationLocationSetting Value { get; } = new() {ApplicationConfigurationFolder = "/test"};
    }

    public void Dispose()
    {
        _connectSubject.Dispose();
        _disconnectSubject.Dispose();
    }
}
