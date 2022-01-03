using NetDaemon.AppModel.Internal.Compiler;
using NetDaemon.AppModel.Internal.Config;
using System.Reflection;

namespace NetDaemon.AppModel;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppsFrom(this IServiceCollection services, Assembly assembly)
    {
        services.AddSingleton<IAssemblyResolver>(new AssemblyResolver(assembly));
        return services;
    }

    public static IServiceCollection AddAppModelLocalAssembly(this IServiceCollection services)
    {
        services
            .AddAppModel()
            .AddAppsFrom(Assembly.GetCallingAssembly());
        return services;
    }

    public static IServiceCollection AddAppModelDynamicCompliedAssembly(this IServiceCollection services)
    {
        services
            .AddAppModel()
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
            .AddAppTypeResolver()
            .AddScopedAppServices()
            .AddConfigManagement();
        return services;
    }

    internal static IServiceCollection AddAppTypeResolver(this IServiceCollection services)
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