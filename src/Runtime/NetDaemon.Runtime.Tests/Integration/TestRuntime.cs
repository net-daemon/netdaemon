using System.Reflection;
using LocalApps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.AppModel;
using NetDaemon.Runtime.Internal;
using NetDaemon.Runtime.Tests.Helpers;

namespace NetDaemon.Runtime.Tests;

public class TestRuntime
{
    [Fact]
    public async Task TestApplicationIsLoaded()
    {
        var timedCancellationSource = new CancellationTokenSource(5000);
        var haRunner = new HomeAssistantRunnerMock();

        var hostBuilder = GetDefaultHostBuilder();
        var host = hostBuilder.ConfigureServices((_, services) =>
            services
                .AddSingleton(haRunner.Object)
                .AddAppsFromAssembly(Assembly.GetExecutingAssembly())
        ).Build();

        var runnerTask = host.RunAsync(timedCancellationSource.Token);

        haRunner.MockConnect();
        var service = (NetDaemonRuntime) host.Services.GetService<IRuntime>()!;
        var instances = service.ApplicationInstances;

        instances.Where(n => n.Id == "LocalApps.LocalApp").Should().NotBeEmpty();
        await timedCancellationSource.CancelAsync();
        await runnerTask.ConfigureAwait(false);
    }

    [Fact]
    public async Task TestApplicationReactToNewEvents()
    {
        var timedCancellationSource = new CancellationTokenSource(-1);
        var haRunner = new HomeAssistantRunnerMock();

        var hostBuilder = GetDefaultHostBuilder();
        var host = hostBuilder.ConfigureServices((_, services) =>
            services
                .AddSingleton(haRunner.Object)
                .AddNetDaemonApp<LocalApp>()
                .AddTransient<IObservable<HassEvent>>(_ => haRunner.ClientMock.ConnectionMock.HomeAssistantEventMock)
        ).Build();

        var runnerTask = host.StartAsync(timedCancellationSource.Token);
        haRunner.MockConnect();
        await runnerTask.ConfigureAwait(false);

        haRunner.ClientMock.ConnectionMock.AddStateChangeEvent(
            new HassState
            {
                EntityId = "binary_sensor.mypir",
                State = "off"
            },
            new HassState
            {
                EntityId = "binary_sensor.mypir",
                State = "on"
            });

        await Task.Delay(100).ConfigureAwait(false);
        // stopping the host will also flush any event queues
        await host.StopAsync(timedCancellationSource.Token).ConfigureAwait(false);

        haRunner.ClientMock.ConnectionMock.Verify(
            n => n.SendCommandAsync(It.IsAny<CallServiceCommand>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TestApplicationReactToNewEventsAndThrowException()
    {
        var timedCancellationSource = new CancellationTokenSource(5000);
        var haRunner = new HomeAssistantRunnerMock();

        var hostBuilder = GetDefaultHostBuilder();
        var host = hostBuilder.ConfigureServices((_, services) =>
            services
                .AddSingleton(haRunner.Object)
                .AddNetDaemonApp<LocalApp>()
        ).Build();

        var runnerTask = host.StartAsync(timedCancellationSource.Token);
        haRunner.MockConnect();

        haRunner.ClientMock.ConnectionMock.AddStateChangeEvent(
            new HassState
            {
                EntityId = "binary_sensor.mypir_creates_fault",
                State = "off"
            },
            new HassState
            {
                EntityId = "binary_sensor.mypir_creates_fault",
                State = "on"
            });

        await timedCancellationSource.CancelAsync();
        await runnerTask.ConfigureAwait(false);
    }


    [Fact]
    public async Task TestShutdownHostShutDownApps()
    {
        var timedCancellationSource = new CancellationTokenSource();
        var haRunner = new HomeAssistantRunnerMock();

        var disposableApp = new Mock<IAsyncDisposable>();

        var host = Host.CreateDefaultBuilder()
            .UseNetDaemonRuntime()
            .ConfigureServices((_, services) =>
                services
                    .AddSingleton(haRunner.Object)
                    .AddNetDaemonApp(_ => disposableApp.Object))
            .Build();
        var runnerTask = host.StartAsync(timedCancellationSource.Token);

        haRunner.MockConnect();
        await runnerTask.WaitAsync(timedCancellationSource.Token).ConfigureAwait(false);

        await host.StopAsync(timedCancellationSource.Token).WaitAsync(timedCancellationSource.Token).ConfigureAwait(false);
        host.Dispose();

        disposableApp.Verify(m=>m.DisposeAsync(), Times.Once);
    }

    private static IHostBuilder GetDefaultHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .UseNetDaemonRuntime();
    }
}
