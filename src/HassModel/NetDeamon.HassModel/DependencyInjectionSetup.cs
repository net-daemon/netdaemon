﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.Client.Common.HomeAssistant.Model;
using NetDaemon.HassModel.Common;
using NetDaemon.Infrastructure.ObservableHelpers;

namespace NetDaemon.HassModel
{
    /// <summary>
    /// Setup methods for services configuration 
    /// </summary>
    public static class DependencyInjectionSetup
    {
        /// <summary>
        /// Registers services for using the IHaContext interface scoped to NetDemonApps
        /// </summary>
        public static IHostBuilder UseAppScopedHaContext(this IHostBuilder hostBuilder)
        {
            if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));

            return hostBuilder
                .ConfigureServices((_, services) => services.AddScopedHaContext());
        }

        /// <summary>
        /// Registers services for using the IHaContext interface scoped to NetDemonApps
        /// </summary>
        public static IHostBuilder UseAppScopedHaContext2(this IHostBuilder hostBuilder)
        {
            if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));

            return hostBuilder
                .ConfigureServices((_, services) => services.AddScopedHaContext2());
        }

        internal static void AddScopedHaContext(this IServiceCollection services)
        {
            services.AddSingleton<Internal.HassClient.EntityStateCache>();
            services.AddSingleton<Internal.HassClient.EntityAreaCache>();
            services.AddScoped<Internal.HassClient.AppScopedHaContextProvider>();
            services.AddTransient<IHaContext>(s => s.GetRequiredService<Internal.HassClient.AppScopedHaContextProvider>());
        }
        internal static void AddScopedHaContext2(this IServiceCollection services)
        {
            services.AddSingleton<Internal.Client.EntityStateCache>();
            services.AddSingleton<Internal.Client.EntityAreaCache>();
            services.AddScoped<Internal.Client.AppScopedHaContextProvider>();
            services.AddTransient<ICacheManager, Internal.Client.CacheManager>();
            services.AddTransient<IHaContext>(s => s.GetRequiredService<Internal.Client.AppScopedHaContextProvider>());
            services.AddScoped<QueuedObservable<HassEvent>>();
            services.AddScoped<IQueuedObservable<HassEvent>>(s => s.GetRequiredService<QueuedObservable<HassEvent>>());
        }

        /// <summary>
        /// Performs async initialization of the HassModel services using old client
        /// </summary>
        public static async Task InitializeAsync(IServiceProvider sp, CancellationToken cancellationToken)
        {
            await sp.GetRequiredService<Internal.HassClient.EntityAreaCache>().InitializeAsync().ConfigureAwait(false);
            await sp.GetRequiredService<Internal.HassClient.EntityStateCache>().InitializeAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}