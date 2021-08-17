using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NetDaemon.Common.ModelV3;
using NetDaemon.Daemon;

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
                    services.TryAddSingleton<IHaContext>(provider => new HaContextProvider(provider.GetRequiredService<NetDaemonHost>()));
                });
            }
        }
}