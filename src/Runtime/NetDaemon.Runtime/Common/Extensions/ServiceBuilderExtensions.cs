using NetDaemon.AppModel;
using NetDaemon.Runtime.Internal;

namespace NetDaemon.Runtime;

public static class ServiceBuilderExtensions
{
    public static IServiceCollection AddNetDaemonStateManager(this IServiceCollection services)
    {
        services.AddSingleton<AppStateManager>();
        services.AddSingleton<IAppStateManager>(s => s.GetRequiredService<AppStateManager>());
        services.AddSingleton<IHandleHomeAssistantAppStateUpdates>(s => s.GetRequiredService<AppStateManager>());

        return services;
    }
}