using System;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Common;
using NetDaemon.Daemon.Services;

namespace NetDaemon
{
    /// <summary>
    /// Provides setup for services from NetDaemon.App
    /// </summary>
    public static class DependencyInjectionSetup
    {
        /// <summary>
        /// Registers services from NetDaemon.App assembly
        /// </summary>
        public static IServiceCollection AddNetDaemonAppServices(this IServiceCollection services)
        {
            // The ApplicationScope is used as a way to make the ApplicationContext resolvable per Scope
            
            services.AddScoped<ApplicationScope>();
            services.AddScoped(s => s.GetRequiredService<ApplicationScope>().ApplicationContext);
            services.AddScoped<IApplicationMetadata>(s => s.GetRequiredService<ApplicationScope>().ApplicationContext ?? throw new InvalidOperationException("ApplicationMetaData not yet initialized"));
            services.AddScoped<IPersistenceService, ApplicationPersistenceService>();
            services.AddScoped<RuntimeInfoManager>();

            return services;
        }
    }
}