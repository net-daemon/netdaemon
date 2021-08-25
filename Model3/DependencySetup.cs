using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Model3.ModelV3;
using NetDaemon.Common.ModelV3;

namespace Model3
{
    public static  class DependencySetup
    {
        public static IHostBuilder UseNetDaemonV3(this IHostBuilder hostBuilder)
        {
            if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));

            return hostBuilder
                .ConfigureServices((_, services) =>
                {
                    // TODO: make this scoped and just the StateManager as a singleton
                    services.TryAddTransient<HaContextProvider>();
                    services.TryAddSingleton<IHaContext>(provider => provider.GetRequiredService<HaContextProvider>());
                    services.AddTransient<IEventProvider, TypedEventProvider>();
                });
            }
        }
}