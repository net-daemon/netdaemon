using Microsoft.Extensions.Configuration;
using NetDaemon.AppModel;
using NetDaemon.HassModel;
using NetDaemon.Runtime.Internal;

namespace NetDaemon.Runtime;

/// <summary>
/// NetDaemon.Runtime Extension methods for IServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the NetDaemon Runtime Services to a IServiceCollection
    /// </summary>
    public static IServiceCollection AddNetDaemonRuntime(this IServiceCollection services)
    {
        services.AddScopedHaContext();
        services.AddLogging();
        services.AddHostedService<RuntimeService>();
        services.AddHomeAssistantClient();
        services.AddOptions<HomeAssistantSettings>().BindConfiguration("HomeAssistant");
        services.AddSingleton<NetDaemonRuntime>();
        services.AddSingleton<IRuntime>(provider => provider.GetRequiredService<NetDaemonRuntime>());
        services.AddSingleton<INetDaemonRuntime>(provider => provider.GetRequiredService<NetDaemonRuntime>());
        return services;
    }

    /// <summary>
    /// Loads NetDaemon configuration section
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="config">The current <see cref="IConfiguration" /> instance</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection ConfigureNetDaemonServices(this IServiceCollection services)
    {
         services.AddOptions<AppConfigurationLocationSetting>().BindConfiguration("NetDaemon");
         return services;
    }
}
