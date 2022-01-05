using Microsoft.Extensions.Configuration;
using NetDaemon.AppModel;
using NetDaemon.HassModel;
using NetDaemon.Runtime.Internal;

namespace NetDaemon.Runtime;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseNetDaemonRuntime(this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .UseAppScopedHaContext2()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging();
                services.Configure<ApplicationLocationSetting>(context.Configuration.GetSection("NetDaemon"));

                services.Configure<HomeAssistantSettings>(context.Configuration.GetSection("HomeAssistant"));
                services.AddHostedService<RuntimeService>();
                services.AddHomeAssistantClient();

                services.AddSingleton<IRuntime, NetDaemonRuntime>();
            })
            .ConfigureAppConfiguration((ctx, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json");
                config.AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", true);
                config.AddEnvironmentVariables();
                var c = config.Build();
                var locationSetting = c.GetSection("NetDaemon").Get<ApplicationLocationSetting>();
                config.AddYamlAppConfig(
                    locationSetting.ApplicationFolder);
            });
    }
}