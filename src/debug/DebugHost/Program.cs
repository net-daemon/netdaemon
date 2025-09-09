using Microsoft.Extensions.Hosting;
using NetDaemon.Runtime;
using NetDaemon.AppModel;
using System.Reflection;
using NetDaemon.Extensions.Logging;
using NetDaemon.Extensions.Tts;
using NetDaemon.Extensions.MqttEntityManager;

var builder = Host.CreateApplicationBuilder(args);
builder.UseNetDaemonAppSettings();

builder.Services
    .AddNetDaemonRuntime()
    .AddNetDaemonDefaultLogging()
    .AddNetDaemonTextToSpeech()
    .AddNetDaemonMqttEntityManagement()
    .AddAppsFromAssembly(Assembly.GetExecutingAssembly())
    .AddNetDaemonStateManager();

await builder.Build().RunAsync();
