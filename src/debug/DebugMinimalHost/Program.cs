using System.Reflection;
using Microsoft.Extensions.Hosting;
using NetDaemon.AppModel;
using NetDaemon.Runtime;
using NetDaemon.HassModel;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddNetDaemonRuntime()

    // Classic way: Scan the assembly for [NetDaemonApp] classes
    .AddAppsFromAssembly(Assembly.GetExecutingAssembly())

    // Add any class as an App (does not need [NetDaemonApp])
    .AddNetDaemonApp<MyApp>()

    // new way, does not need a type at all
    .AddNetDaemonApp("LightWeightApp", (IHaContext ha) =>
        ha.Entity("input_button.test_button")
            .StateAllChanges()
            .Subscribe(_ => ha.Entity("input_boolean.dummy_switch").CallService("toggle")));

await builder.Build().RunAsync();


class MyApp
{
    public MyApp(IHaContext ha)
    {
        ha.Entity("input_boolean.test_toggle").CallService("toggle");
    }
}
