using Microsoft.Extensions.Hosting;
using NetDaemon.Infrastructure.ObservableHelpers;

namespace NetDaemon.HassModel;

/// <summary>
/// Setup methods for services configuration
/// </summary>
public static class DependencyInjectionSetup
{
    /// <summary>
    /// Registers services for using the IHaContext interface scoped to NetDemonApps
    /// </summary>
    public static IHostBuilder UseAppScopedHaContext(this IHostBuilder hostBuilder)
    {

        ArgumentNullException.ThrowIfNull(hostBuilder, nameof(hostBuilder));

        return hostBuilder
            .ConfigureServices((_, services) => services.AddScopedHaContext());
    }

    internal static void AddScopedHaContext(this IServiceCollection services)
    {
        services.AddSingleton<EntityStateCache>();
        services.AddSingleton<EntityAreaCache>();
        services.AddScoped<AppScopedHaContextProvider>();
        services.AddScoped<BackgroundTaskTracker>();
        services.AddScoped<IBackgroundTaskTracker>(s => s.GetRequiredService<BackgroundTaskTracker>());
        services.AddTransient<ICacheManager, CacheManager>();
        services.AddTransient<IHaContext>(s => s.GetRequiredService<AppScopedHaContextProvider>());
        services.AddScoped<QueuedObservable<HassEvent>>();
        services.AddScoped<IQueuedObservable<HassEvent>>(s => s.GetRequiredService<QueuedObservable<HassEvent>>());
        services.AddScoped<TriggerManager>();
        services.AddTransient<ITriggerManager>(s => s.GetRequiredService<TriggerManager>());
    }
}
