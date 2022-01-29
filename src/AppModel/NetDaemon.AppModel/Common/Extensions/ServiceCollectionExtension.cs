using System.Reflection;
using NetDaemon.AppModel.Internal.Compiler;
using NetDaemon.AppModel.Internal.Config;

namespace NetDaemon.AppModel;

/// <summary>
///     ServiceCollection extensions
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds applications from the specified assembly
    /// </summary>
    /// <param name="services">Services</param>
    /// <param name="assembly">The assembly loading apps from</param>
    public static IServiceCollection AddAppsFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        return services
            .AddAppModelIfNotExist()
            .AddAppTypeResolverIfNotExist()
            .AddSingleton<IAssemblyResolver>(new AssemblyResolver(assembly));
    }

    /// <summary>
    ///     Add a single app
    /// </summary>
    /// <param name="services">Services</param>
    /// <param name="type">The type of the app to add</param>
    public static IServiceCollection AddAppFromType(this IServiceCollection services, Type type)
    {
        return services
            .AddAppModelIfNotExist()
            .AddSingleton<IAppTypeResolver>(new SingleAppResolver(type));
    }

    /// <summary>
    ///     Add apps from c# source code using the configuration source to find path
    /// </summary>
    /// <param name="services">Services</param>
    public static IServiceCollection AddAppsFromSource(this IServiceCollection services, bool useDebug = false)
    {
        // We make sure we only add AppModel services once
        services
            .AddAppModelIfNotExist()
            .AddAppTypeResolverIfNotExist()
            .AddSingleton<Compiler>()
            .AddSingleton<ICompiler>(s => s.GetRequiredService<Compiler>())
            .AddSingleton<SyntaxTreeResolver>()
            .AddSingleton<ISyntaxTreeResolver>(s => s.GetRequiredService<SyntaxTreeResolver>())
            .AddOptions<CompileSettings>().Configure(settings => settings.UseDebug = useDebug);

        // We need to compile it here so we can dynamically add the service providers
        var assemblyResolver =
            ActivatorUtilities.CreateInstance<DynamicallyCompiledAssemblyResolver>(services.BuildServiceProvider());
        services.RegisterDynamicFunctions(assemblyResolver.GetResolvedAssembly());
        // And not register the assembly resolver that will have the assembly already compiled
        services.AddSingleton(assemblyResolver);
        services.AddSingleton<IAssemblyResolver>(s => s.GetRequiredService<DynamicallyCompiledAssemblyResolver>());
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

    internal static IServiceCollection AddAppModelIfNotExist(this IServiceCollection services)
    {
        // Check if we already registered
        if (services.Any(n => n.ImplementationType == typeof(AppModelImpl)))
            return services;

        services
            .AddSingleton<AppModelImpl>()
            .AddSingleton<IAppModel>(s => s.GetRequiredService<AppModelImpl>())
            .AddTransient<AppModelContext>()
            .AddTransient<IAppModelContext>(s => s.GetRequiredService<AppModelContext>())
            .AddScopedConfigurationBinder()
            .AddScopedAppServices()
            .AddConfigManagement();
        return services;
    }

    internal static IServiceCollection AddAppTypeResolverIfNotExist(this IServiceCollection services)
    {
        // Check if we already registered
        if (services.Any(n => n.ImplementationType == typeof(AppTypeResolver)))
            return services;

        services
            .AddSingleton<AppTypeResolver>()
            .AddSingleton<IAppTypeResolver>(s => s.GetRequiredService<AppTypeResolver>());
        return services;
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
