using Microsoft.Extensions.Configuration;
using NetDaemon.AppModel;

namespace NetDaemon.Runtime;

/// <summary>
/// NetDaemon.Runtime Extension methods for IHostBuilder
/// </summary>
public static class HostBuilderExtensions
{
    /// <summary>
    /// Call this method to load NetDaemon Yaml settings, and to register 'ConfigureNetDaemonServices' in the service collection
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <remarks>
    /// UseNetDaemonAppSettings has several responsibilities:
    ///  - Register appsettings.json to the host configuration
    ///  - Register all the yaml settings from the path set in the current configuration to the configuration provider
    ///  - Call 'ConfigureNetDaemonServices' in the service collection
    ///
    /// You can call these methods separately if you want to do something else in between, or if you're calling any of these methods already.
    /// Change `UseNetDaemonAppSettings` to `.RegisterAppSettingsJsonToHost().RegisterYamlSettings()` and call `ConfigureNetDaemonServices(context.Configuration)` in ConfigureServices.
    /// </remarks>
    public static IHostBuilder UseNetDaemonAppSettings(this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .RegisterAppSettingsJsonToHost()
            .RegisterYamlSettings()
            .ConfigureServices((_, services)
                => services.ConfigureNetDaemonServices()
            );
    }

    /// <summary>
    /// Adds the yaml appsettings to the configuration
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T UseNetDaemonAppSettings<T>(this T hostBuilder) where T : IHostApplicationBuilder
    {
        hostBuilder.Configuration.AddYamlAppConfigs(hostBuilder.Configuration);

        return hostBuilder;
    }


    /// <summary>
    /// Registers appsettings.json to the host configuration
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <remarks>This enables using data from the appsettings.json in the `ConfigureAppConfiguration` call</remarks>
    public static IHostBuilder RegisterAppSettingsJsonToHost(this IHostBuilder hostBuilder)
    {
        // The default Host Builders already add appsettings.json, not sure why this is needed here
        return hostBuilder.ConfigureHostConfiguration(config =>
        {
            config.AddJsonFile("appsettings.json", optional: true);
        });
    }

    /// <summary>
    /// Register all the yaml settings from the path set in the current configuration to the configuration provider
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <remarks>Call `RegisterAppSettingsJsonToHost()` before, using this method.</remarks>
    public static IHostBuilder RegisterYamlSettings(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddYamlAppConfigs(context.Configuration);
        });
    }

    /// <summary>
    /// Adds the NetDaemon Runtime Services to a HostBuilder
    /// </summary>
    public static IHostBuilder UseNetDaemonRuntime(this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .ConfigureServices((_, services) =>
            {
                services.AddNetDaemonRuntime();
            });
    }
}
