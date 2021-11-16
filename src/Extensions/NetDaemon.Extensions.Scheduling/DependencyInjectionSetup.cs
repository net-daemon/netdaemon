using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NetDaemon.Extensions.Scheduler
{
    /// <summary>
    ///     Implements depenedency injection for the scheduler
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
            return services;
        }
    }
}