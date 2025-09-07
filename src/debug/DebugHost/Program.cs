using Microsoft.Extensions.Hosting;
using NetDaemon.Runtime;
using NetDaemon.AppModel;
using System.Reflection;
using NetDaemon.Extensions.Logging;
using NetDaemon.Extensions.Tts;
using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.HassModel;

var builder = Host.CreateApplicationBuilder(args);
builder.UseNetDaemonAppSettings();

builder.Services
    .AddNetDaemonRuntime()
    .AddNetDaemonDefaultLogging()
    .AddNetDaemonTextToSpeech()
    .AddNetDaemonMqttEntityManagement()
    .AddAppsFromAssembly(Assembly.GetExecutingAssembly())
    .AddNetDaemonStateManager()

    .AddNetDaemonApp("LightWeightApp", (IHaContext ha) =>
        ha.Entity("input_button.test_button")
            .StateAllChanges()
            .Subscribe(_ => ha.Entity("input_boolean.dummy_switch").CallService("toggle")));

await builder.Build().RunAsync();
