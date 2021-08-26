using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Model3.ModelV3;
using NetDaemon.Common.ModelV3;

namespace Model3
{
    public static class DependencyInjectionSetup
    {
        public static IHostBuilder UseHaContext(this IHostBuilder hostBuilder)
        {
            if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));

            return hostBuilder
                .ConfigureServices((_, services) =>
                {
                    services.AddScoped<IHaContext, HaContextProvider>();
                    services.AddSingleton<EntityStateCache>();
                    services.AddTransient<IEventProvider, TypedEventProvider>();
                });
        }
    }
}