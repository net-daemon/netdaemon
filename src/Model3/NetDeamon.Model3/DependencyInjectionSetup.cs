using System;
using System.Linq;
using JoySoftware.HomeAssistant.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.Model3.Common;
using NetDaemon.Model3.Internal;

namespace NetDaemon.Model3
{
    public static class DependencyInjectionSetup
    {
        public static IHostBuilder UseHaContext(this IHostBuilder hostBuilder)
        {
            if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));

            return hostBuilder
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton<EntityStateCache>();
                    services.AddScoped<HaContextProvider>();
                    services.AddTransient<IHaContext>(s => s.GetRequiredService<HaContextProvider>());
                    services.AddTransient<IEventProvider>(s => s.GetRequiredService<HaContextProvider>());
                });
        }
    }
}