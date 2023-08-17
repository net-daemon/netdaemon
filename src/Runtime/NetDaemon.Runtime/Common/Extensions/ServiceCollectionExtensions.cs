using Microsoft.Extensions.Configuration;
using NetDaemon.AppModel;

namespace NetDaemon.Runtime;

/// <summary>
/// NetDaemon.Runtime Extension methods for IServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Loads NetDaemon and HomeAssistant configuration sections
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="config">The current <see cref="IConfiguration" /> instance</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection ConfigureNetDaemonServices(this IServiceCollection services, IConfiguration config)
    {
        return services.Configure<AppConfigurationLocationSetting>(config.GetSection("NetDaemon"))
                       .Configure<HomeAssistantSettings>(config.GetSection("HomeAssistant"));
        // todo: maybe remove 'HomeAssistant' section this here, is this method really needed? If we remove this we can inline the rest
    }
}