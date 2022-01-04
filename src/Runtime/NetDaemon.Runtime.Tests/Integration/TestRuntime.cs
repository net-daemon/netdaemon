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