using NetDaemon.AppModel.Common.Settings;
using NetDaemon.AppModel.Internal.Compiler;
using NetDaemon.AppModel.Internal.Config;
using NetDaemon.AppModel.Internal;

namespace NetDaemon.AppModel.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppModelLocalAssembly(this IServiceCollection services)
    {
        services
            .AddAppModel()
            .AddSingleton<LocalAssemblyAppTypeResolver>()
            .AddSingleton<IAppTypeResolver>(s => s.GetRequiredService<LocalAssemblyAppTypeResolver>());
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
            .AddSingleton<DynamicCompiledAssemblyAppTypeResolver>()
            .AddSingleton<IAppTypeResolver>(s => s.GetRequiredService<DynamicCompiledAssemblyAppTypeResolver>());
        return services;
    }

    private static IServiceCollection AddAppModel(this IServiceCollection services)
    {
        services
            .AddSingleton<AppModelImpl>()
            .AddSingleton<IAppModel>(s => s.GetRequiredService<AppModelImpl>())
            .AddSingleton<ConfigurationBinding>()
            .AddSingleton<IConfigurationBinding>(s => s.GetRequiredService<ConfigurationBinding>())
            .AddScopedAppServices()
            .AddConfigManagement();
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