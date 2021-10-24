using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Internal;

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

        internal static void AddScopedHaContext(this IServiceCollection services)
        {
            services.AddSingleton<EntityStateCache>();
            services.AddScoped<AppScopedHaContextProvider>();
            services.AddTransient<IHaContext>(s => s.GetRequiredService<AppScopedHaContextProvider>());
            services.AddTransient<IEventProvider>(s => s.GetRequiredService<AppScopedHaContextProvider>());
        }
        
        /// <summary>
        /// Performs async initialization of the HassModel services 
        /// </summary>
        public static Task InitializeAsync(IServiceProvider sp, CancellationToken cancellationToken)
        {
            return sp.GetRequiredService<EntityStateCache>().InitializeAsync(cancellationToken);
        }
    }
}