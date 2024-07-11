using Microsoft.Extensions.Hosting;

namespace NetDaemon.HassModel;

/// <summary>
/// Setup methods for services configuration
/// </summary>
public static class DependencyInjectionSetup
{
    /// <summary>
    /// Registers services for using the IHaContext interface scoped to NetDaemonApps
    /// </summary>
    public static IHostBuilder UseAppScopedHaContext(this IHostBuilder hostBuilder)
    {

        ArgumentNullException.ThrowIfNull(hostBuilder, nameof(hostBuilder));

        return hostBuilder
            .ConfigureServices((_, services) => services.AddScopedHaContext());
    }

    /// <summary>
    /// Registers services for using the IHaContext interface scoped to NetDaemonApps
    /// </summary>
    public static void AddScopedHaContext(this IServiceCollection services)
    {
        services.AddSingleton<EntityStateCache>();
        services.AddSingleton<RegistryCache>();
        services.AddScoped<HaRegistry>();

        services.AddScoped<AppScopedHaContextProvider>();
        services.AddScoped<IHaRegistry>(sp => sp.GetRequiredService<AppScopedHaContextProvider>().Registry);
        services.AddScoped<IHaRegistryNavigator>(sp => sp.GetRequiredService<AppScopedHaContextProvider>().Registry);
        services.AddScoped<BackgroundTaskTracker>();
        services.AddScoped<IBackgroundTaskTracker>(s => s.GetRequiredService<BackgroundTaskTracker>());
        services.AddTransient<ICacheManager, CacheManager>();
        services.AddTransient<IHaContext>(s => s.GetRequiredService<AppScopedHaContextProvider>());
        services.AddScoped<TriggerManager>();
        services.AddTransient<ITriggerManager>(s => s.GetRequiredService<TriggerManager>());
    }
}
