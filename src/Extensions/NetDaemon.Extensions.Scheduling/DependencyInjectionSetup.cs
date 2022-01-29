using System.Reactive.Concurrency;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NetDaemon.Extensions.Scheduler;

/// <summary>
///     Implements dependency injection for the scheduler
/// </summary>
public static class DependencyInjectionSetup
{
    /// <summary>
    ///     Adds scheduling capabilities through dependency injection
    /// </summary>
    /// <param name="services">Provided service collection</param>
    public static IServiceCollection AddNetDaemonScheduler(this IServiceCollection services)
    {
        services.AddScoped<INetDaemonScheduler, NetDaemonScheduler>();
        services.AddScoped<IScheduler>(s => new DisposableScheduler(DefaultScheduler.Instance.WrapWithLogger(s.GetRequiredService<ILogger<IScheduler>>())));
        return services;
    }
}