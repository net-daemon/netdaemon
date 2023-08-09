using System.Reflection;
using NetDaemon.AppModel.Internal.AppAssemblyProviders;
using NetDaemon.AppModel.Internal.AppFactories;
using NetDaemon.AppModel.Internal.AppFactoryProviders;
using NetDaemon.AppModel.Internal.Compiler;
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
            .AddAppModelIfNotExist()
            .AddSingleton<IAppFactoryProvider>(SingleAppFactoryProvider.Create(factoryFunc, id, focus));
    }

    /// <summary>
    ///     Adds applications from the specified assembly
    /// </summary>
    /// <param name="services">Services</param>
    /// <param name="assembly">The assembly loading apps from</param>
    public static IServiceCollection AddAppsFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        return services
            .AddAppModelIfNotExist()
            .AddAppFactoryIfNotExists()
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
            .AddAppModelIfNotExist()
            .AddSingleton(SingleAppFactoryProvider.Create(type));
    }

    /// <summary>
    ///     Add apps from c# source code using the configuration source to find path
    /// </summary>
    /// <param name="services">Services</param>
    /// <param name="useDebug">Override UseDebug in CompileSettings</param>
    public static IServiceCollection AddAppsFromSource(this IServiceCollection services, bool useDebug = false)
    {
        // We make sure we only add AppModel services once
        services
            .AddAppModelIfNotExist()
            .AddAppFactoryIfNotExists()
            .AddSingleton<Compiler>()
            .AddSingleton<ICompiler>(s => s.GetRequiredService<Compiler>())
            .AddSingleton<SyntaxTreeResolver>()
            .AddSingleton<ISyntaxTreeResolver>(s => s.GetRequiredService<SyntaxTreeResolver>())
            .AddOptions<CompileSettings>().Configure(settings => settings.UseDebug = useDebug);

        // We need to compile it here so we can dynamically add the service providers
        var assemblyResolver = ActivatorUtilities.CreateInstance<DynamicallyCompiledAppAssemblyProvider>(services.BuildServiceProvider());
        services.RegisterDynamicFunctions(assemblyResolver.GetAppAssembly());

        // And now register the assembly resolver that will have the assembly already compiled
        services.AddSingleton(assemblyResolver);
        services.AddSingleton<IAppAssemblyProvider>(s => s.GetRequiredService<DynamicallyCompiledAppAssemblyProvider>());

        return services;
    }

    private static IServiceCollection RegisterDynamicFunctions(this IServiceCollection services, Assembly assembly)
    {
        var methods = assembly.GetTypes().SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
                          .Where(m => m.GetCustomAttribute<ServiceCollectionExtensionAttribute>() != null).ToArray() ??
                      Array.Empty<MethodInfo>();

        if (methods.Any(
                m => m.GetParameters().Length != 1 && m.GetParameters()[0].GetType() != typeof(IServiceProvider)))
            throw new InvalidOperationException(
                "Methods with [ServiceCollectionExtension] Attribute should have exactly one parameter of type IServiceCollection");

        foreach (var methodInfo in methods) methodInfo.Invoke(null, new object?[] {services});

        return services;
    }

    private static IServiceCollection AddAppModelIfNotExist(this IServiceCollection services)
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
            .AddConfigManagement();
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
