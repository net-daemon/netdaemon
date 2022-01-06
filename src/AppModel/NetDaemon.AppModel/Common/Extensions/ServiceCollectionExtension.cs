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
            services.AddAppModel();

        services.AddSingleton<IAssemblyResolver>(new AssemblyResolver(assembly));
        return services;
    }

    public static IServiceCollection AddAppsFromType(this IServiceCollection services, Type type)
    {
        return services.AddAppsFromAssembly(type.Assembly);
    }

    public static IServiceCollection AddAppsFromSource(this IServiceCollection services)
    {
        // We make sure we only add AppModel services once
        if (!services.Any(n => n.ImplementationType == typeof(AppModelImpl)))
            services.AddAppModel();

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
            .AddSingleton<ConfigurationBinding>()
            .AddSingleton<IConfigurationBinding>(s => s.GetRequiredService<ConfigurationBinding>())
            .AddTransient<AppModelContext>()
            .AddTransient<IAppModelContext>(s => s.GetRequiredService<AppModelContext>())
            .AddAppTypeResolver()
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