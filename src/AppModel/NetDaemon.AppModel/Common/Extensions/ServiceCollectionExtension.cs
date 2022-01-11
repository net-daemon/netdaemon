using System.Reflection;
using NetDaemon.AppModel.Internal.Compiler;
using NetDaemon.AppModel.Internal.Config;

namespace NetDaemon.AppModel;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppsFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        // We make sure we only add AppModel services once
        if (!services.Any(n => n.ImplementationType == typeof(AppModelImpl)))
        {
            services
                .AddAppModel()
                .AddAppTypeResolver();
        }

        services.AddSingleton<IAssemblyResolver>(new AssemblyResolver(assembly));
        return services;
    }

    public static IServiceCollection AddAppFromType(this IServiceCollection services, Type type)
    {
        // We make sure we only add AppModel services once
        if (!services.Any(n => n.ImplementationType == typeof(AppModelImpl)))
            services.AddAppModel();

        return services.AddSingleton<IAppTypeResolver>(new SingleAppResolver(type));
    }

    public static IServiceCollection AddAppsFromSource(this IServiceCollection services)
    {
        // We make sure we only add AppModel services once
        if (!services.Any(n => n.ImplementationType == typeof(AppModelImpl)))
        {
            services
                .AddAppModel()
                .AddAppTypeResolver();
        }

        services
            .AddSingleton<CompilerFactory>()
            .AddSingleton<ICompilerFactory>(s => s.GetRequiredService<CompilerFactory>())
            .AddSingleton<SyntaxTreeResolver>()
            .AddSingleton<ISyntaxTreeResolver>(s => s.GetRequiredService<SyntaxTreeResolver>())
            .AddSingleton<DynamicallyCompiledAssemblyResolver>()
            .AddSingleton<IAssemblyResolver>(s => s.GetRequiredService<DynamicallyCompiledAssemblyResolver>());
        return services;
    }

    private static IServiceCollection AddAppModel(this IServiceCollection services)
    {
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

    private static IServiceCollection AddAppTypeResolver(this IServiceCollection services)
    {
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