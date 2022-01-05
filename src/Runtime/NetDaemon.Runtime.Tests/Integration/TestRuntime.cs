using System.Reflection;
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
        var haRunner = new HomeAssistantRunnerMock(timedCancellationSource.Token);

        var hostBuilder = GetDefaultHostBuilder("Fixtures");
        var host = hostBuilder.ConfigureServices((_, services) =>
           services
               .AddSingleton(haRunner.Object)
               .AddAppsFromAssembly(Assembly.GetExecutingAssembly())
        ).Build();


        var runnerTask = host.RunAsync();
        while (!haRunner.ConnectMock.HasObservers) { await Task.Delay(10); }
        haRunner.ConnectMock.OnNext(haRunner.ClientMock.ConnectionMock.Object);
        var service = (NetDaemonRuntime)host.Services.GetService<IRuntime>()!;
        var instances = service?.ApplicationInstances;

        instances!.Where(n => n.Id == "LocalApps.LocalApp").Should().NotBeEmpty();
        timedCancellationSource.Cancel();
        await runnerTask.ConfigureAwait(false);
    }
    // public ISetup<T> Setup(Expression<Action<T>> expression);
    // public ISetup<T, TResult> Setup<TResult>(Expression<Func<T, TResult>> expression);

    [Fact]
    public async Task TestApplicationReactToNewEvents()
    {
        var timedCancellationSource = new CancellationTokenSource(5000);
        var haRunner = new HomeAssistantRunnerMock(timedCancellationSource.Token);

        var invocationTask = haRunner.ClientMock.ConnectionMock.WaitForInvocation(n =>
            n.SendCommandAndReturnResponseAsync<CallServiceCommand, object>(
                It.IsAny<CallServiceCommand>(),
                It.IsAny<CancellationToken>()
        ));

        var hostBuilder = GetDefaultHostBuilder("Fixtures");
        var host = hostBuilder.ConfigureServices((_, services) =>
           services
               .AddSingleton(haRunner.Object)
               .AddAppsFromAssembly(Assembly.GetExecutingAssembly())
        ).Build();


        var runnerTask = host.RunAsync();
        while (!haRunner.ConnectMock.HasObservers) { await Task.Delay(10); }
        haRunner.ConnectMock.OnNext(haRunner.ClientMock.ConnectionMock.Object);
        var service = (NetDaemonRuntime)host.Services.GetService<IRuntime>()!;
        var instances = service?.ApplicationInstances;

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

        await invocationTask.ConfigureAwait(false);

        haRunner.ClientMock.ConnectionMock.Verify(
            n => n.SendCommandAndReturnResponseAsync<CallServiceCommand, object>(It.IsAny<CallServiceCommand>(), It.IsAny<CancellationToken>()), Times.Once);

        timedCancellationSource.Cancel();
        await runnerTask.ConfigureAwait(false);
    }

    [Fact]
    public async Task TestApplicationReactToNewEventsAndThrowException()
    {
        var timedCancellationSource = new CancellationTokenSource(5000);
        var haRunner = new HomeAssistantRunnerMock(timedCancellationSource.Token);

        var hostBuilder = GetDefaultHostBuilder("Fixtures");
        var host = hostBuilder.ConfigureServices((_, services) =>
           services
               .AddSingleton(haRunner.Object)
               .AddAppsFromAssembly(Assembly.GetExecutingAssembly())
        ).Build();


        var runnerTask = host.RunAsync();
        while (!haRunner.ConnectMock.HasObservers) { await Task.Delay(10); }
        haRunner.ConnectMock.OnNext(haRunner.ClientMock.ConnectionMock.Object);
        var service = (NetDaemonRuntime)host.Services.GetService<IRuntime>()!;
        var instances = service?.ApplicationInstances;

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

        timedCancellationSource.Cancel();
        await runnerTask.ConfigureAwait(false);
    }

    private static IHostBuilder GetDefaultHostBuilder(string path)
    {
        return Host.CreateDefaultBuilder()
           .UseNetDaemonRuntime()
           .ConfigureServices((_, services) =>
           {
               services.Configure<HostOptions>(hostOptions =>
               {
                   hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
               });
               services.AddTransient<IOptions<ApplicationLocationSetting>>(
                   _ => new FakeOptions(Path.Combine(AppContext.BaseDirectory, path)));
           })
           .ConfigureAppConfiguration((_, config) =>
           {
               config.AddYamlAppConfig(
                   Path.Combine(AppContext.BaseDirectory,
                       path));
           });
    }
}