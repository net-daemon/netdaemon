using Microsoft.Extensions.Configuration;
using NetDaemon.AppModel;
using NetDaemon.HassModel;
using NetDaemon.Runtime.Internal;

namespace NetDaemon.Runtime;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseNetDaemonAppSettings(this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .ConfigureServices((context, services)
                => services.ConfigureNetDaemonServices(context.Configuration)
            )
            .ConfigureAppConfiguration((ctx, config) =>
            {
                // TODO: Most of this seems to be what Host.CreateDefaultBuilder already does 
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json");
                config.AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", true);
                config.AddEnvironmentVariables();
                
                var c = config.Build();
                var locationSetting = c.GetSection("NetDaemon").Get<AppConfigurationLocationSetting>();
                if (locationSetting?.ApplicationConfigurationFolder is not null)
                {
                    var fullPath = Path.GetFullPath(locationSetting.ApplicationConfigurationFolder);
                    config.AddYamlAppConfig(fullPath);
                }

            });
    }

    public static IHostBuilder UseNetDaemonRuntime(this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .UseAppScopedHaContext()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging();
                services.AddHostedService<RuntimeService>();
                services.AddHomeAssistantClient();
                services.Configure<HomeAssistantSettings>(context.Configuration.GetSection("HomeAssistant"));
                services.AddSingleton<IRuntime, NetDaemonRuntime>();
            });
    }
}