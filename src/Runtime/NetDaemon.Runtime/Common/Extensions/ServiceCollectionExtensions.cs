using Microsoft.Extensions.Configuration;
using NetDaemon.AppModel;

namespace NetDaemon.Runtime;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureNetDaemonServices(this IServiceCollection services, IConfiguration config)
    {
        return services.Configure<AppConfigurationLocationSetting>(config.GetSection("NetDaemon"))
                       .Configure<HomeAssistantSettings>(config.GetSection("HomeAssistant"));
    }
}