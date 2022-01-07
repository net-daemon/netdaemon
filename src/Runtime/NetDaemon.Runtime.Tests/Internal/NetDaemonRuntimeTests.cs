using System.Reactive.Subjects;
using NetDaemon.AppModel;
using NetDaemon.Runtime.Internal;
using NetDaemon.Runtime.Tests.Helpers;

namespace NetDaemon.Runtime.Tests.Internal;

public class NetDaemonRuntimeTests
{
    [Fact]
    public async Task TestExecuteAsync()
    {
        var homeAssistantRunnerMock = new Mock<IHomeAssistantRunner>();
        var optionsMock = new Mock<IOptions<HomeAssistantSettings>>();
        var appModelMock = new Mock<IAppModel>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<RuntimeService>>();

        var connectSubject = new Subject<IHomeAssistantConnection>();
        var disconnectSubject = new Subject<DisconnectReason>();
        homeAssistantRunnerMock.SetupGet(n => n.OnConnect).Returns(connectSubject);
        homeAssistantRunnerMock.SetupGet(n => n.OnDisconnect).Returns(disconnectSubject);
        var runtime = new NetDaemonRuntime(
            homeAssistantRunnerMock.Object,
            optionsMock.Object,
            appModelMock.Object,
            serviceProviderMock.Object,
            loggerMock.Object
        );
        var cancelSource = new CancellationTokenSource(5000);
        var task = runtime.ExecuteAsync(cancelSource.Token);

        await connectSubject.WaitForObservers().ConfigureAwait(false);
        connectSubject.HasObservers.Should().BeTrue();
        disconnectSubject.HasObservers.Should().BeTrue();
    }

    [Fact]
    public async Task TestOnConnect()
    {
        var homeAssistantRunnerMock = new Mock<IHomeAssistantRunner>();
        var optionsMock = new Mock<IOptions<HomeAssistantSettings>>();
        var appModelMock = new Mock<IAppModel>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<RuntimeService>>();

        var connectSubject = new Subject<IHomeAssistantConnection>();
        var disconnectSubject = new Subject<DisconnectReason>();
        homeAssistantRunnerMock.SetupGet(n => n.OnConnect).Returns(connectSubject);
        homeAssistantRunnerMock.SetupGet(n => n.OnDisconnect).Returns(disconnectSubject);
        var runtime = new NetDaemonRuntime(
            homeAssistantRunnerMock.Object,
            optionsMock.Object,
            appModelMock.Object,
            serviceProviderMock.Object,
            loggerMock.Object
        );
        var cancelSource = new CancellationTokenSource(5000);
        var task = runtime.ExecuteAsync(cancelSource.Token);

        await connectSubject.WaitForObservers().ConfigureAwait(false);
        connectSubject.HasObservers.Should().BeTrue();
        disconnectSubject.HasObservers.Should().BeTrue();
    }
}