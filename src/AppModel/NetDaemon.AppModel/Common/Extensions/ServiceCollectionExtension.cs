using System.Reflection;
using NetDaemon.AppModel.Internal.AppAssemblyProviders;
using NetDaemon.AppModel.Internal.AppFactoryProviders;

using NetDaemon.AppModel.Internal.Config;

namespace NetDaemon.AppModel;

/// <summary>
///     ServiceCollection extensions
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds a single app that gets instantiated by the provided Func
    /// </summary>
    /// <param name="services">Services</param>
    /// <param name="factoryFunc">The Func used to create the app</param>
    /// <param name="id">The id of the app. This parameter is optional,
    ///                  by default it tries to locate the id from the specified TAppType.</param>
    /// <param name="focus">Whether this app has focus or not. This parameter is optional,
    ///                     by default it tries to check this using the FocusAttribute</param>
    /// <typeparam name="TAppType">The type of the app</typeparam>
    public static IServiceCollection AddNetDaemonApp<TAppType>(
        this IServiceCollection services,
        Func<IServiceProvider, TAppType> factoryFunc,
        string? id = default,
        bool? focus = default) where TAppType : class
    {
        return services
            .AddNetDaemonAppModel()
            .AddSingleton(SingleAppFactoryProvider.Create(factoryFunc, id, focus));
    }

    /// <summary>
    ///     Adds applications from the specified assembly
    /// </summary>
    /// <param name="services">Services</param>
    /// <param name="assembly">The assembly loading apps from</param>
    public static IServiceCollection AddAppsFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        return services
            .AddNetDaemonAppModel()
            .AddSingleton<IAppAssemblyProvider>(new AppAssemblyProvider(assembly));
    }

    /// <summary>
    ///     Add a single app
    /// </summary>
    /// <param name="services">Services</param>
    /// <typeparam name="TAppType">The type of the app to add</typeparam>
    public static IServiceCollection AddNetDaemonApp<TAppType>(this IServiceCollection services)
    {
        return services.AddNetDaemonApp(typeof(TAppType));
    }

    /// <summary>
    ///     Add a single app
    /// </summary>
    /// <param name="services">Services</param>
    /// <param name="type">The type of the app to add</param>
    public static IServiceCollection AddNetDaemonApp(this IServiceCollection services, Type type)
    {
        return services
            .AddNetDaemonAppModel()
            .AddSingleton(SingleAppFactoryProvider.Create(type));
    }

    /// <summary>
    /// Adds the basic requirements for the NetDaemon application model
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddNetDaemonAppModel(this IServiceCollection services)
    {
        // Check if we already registered
        if (services.Any(n => n.ImplementationType == typeof(AppModelImpl)))
            return services;

        services
            .AddSingleton<AppModelImpl>()
            .AddSingleton<IAppModel>(s => s.GetRequiredService<AppModelImpl>())
            .AddTransient<AppModelContext>()
            .AddTransient<IAppModelContext>(s => s.GetRequiredService<AppModelContext>())
            .AddTransient<FocusFilter>()
            .AddScopedConfigurationBinder()
            .AddScopedAppServices()
            .AddConfigManagement()
            .AddAppFactoryIfNotExists();
        return services;
    }

    private static IServiceCollection AddAppFactoryIfNotExists(this IServiceCollection services)
    {
        if (services.Any(descriptor => descriptor.ImplementationType == typeof(AssemblyAppFactoryProvider)))
            return services;

        return services
            .AddSingleton<AssemblyAppFactoryProvider>()
            .AddSingleton<IAppFactoryProvider>(provider => provider.GetRequiredService<AssemblyAppFactoryProvider>());
    }

    private static IServiceCollection AddScopedConfigurationBinder(this IServiceCollection services)
    {
        services
            .AddScoped<ConfigurationBinding>()
            .AddScoped<IConfigurationBinding>(s => s.GetRequiredService<ConfigurationBinding>());
        return services;
    }

    private static IServiceCollection AddScopedAppServices(this IServiceCollection services)
    {
        services
            .AddScoped<ApplicationScope>()
            .AddScoped(s => s.GetRequiredService<ApplicationScope>().ApplicationContext);
        return services;
    }

    private static IServiceCollection AddConfigManagement(this IServiceCollection services)
    {
        services.AddTransient(typeof(IAppConfig<>), typeof(AppConfig<>));
        return services;
    }
}
