using System.Reactive.Concurrency;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetDaemon.Extensions.Scheduler.SunEvents;

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

    /// <summary>
    ///     Adds sun event scheduling capabilities through dependency injection 
    /// </summary>
    /// <param name="latitude">Latitude of location to use for sun event scheduling</param>
    /// <param name="longitude">Longitude of location to use for sun event scheduling</param>
    /// <param name="services">Provided service collection</param>
    public static IServiceCollection AddSunEventScheduler(this IServiceCollection services, decimal latitude, decimal longitude)
    {
        services.AddNetDaemonScheduler();
        services.AddScoped<ISolarCalendar>((services) => new SolarCalendar(new Coordinates(latitude, longitude)));
        services.AddScoped<ISunEventScheduler, SunEventScheduler>();
        return services;
    }
}
