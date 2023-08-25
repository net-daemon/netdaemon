using NetDaemon.AppModel;
using NetDaemon.Runtime.Internal;

namespace NetDaemon.Runtime;

/// <summary>
/// NetDaemon.Runtime Extension methods for IServiceCollection
/// </summary>
public static class ServiceBuilderExtensions
{
    /// <summary>
    /// COnfigures required sercvies to use the NetDaemon Application State manager
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddNetDaemonStateManager(this IServiceCollection services)
    {
        services.AddSingleton<AppStateManager>();
        services.AddSingleton<IAppStateManager>(s => s.GetRequiredService<AppStateManager>());
        services.AddSingleton<IHandleHomeAssistantAppStateUpdates>(s => s.GetRequiredService<AppStateManager>());
        services.AddSingleton<AppStateRepository>();
        services.AddSingleton<IAppStateRepository>(s => s.GetRequiredService<AppStateRepository>());
        return services;
    }
}
