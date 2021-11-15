using System;
using JoySoftware.HomeAssistant.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.Common;
using NetDaemon.Daemon;
using NetDaemon.Extensions.Scheduler;

namespace NetDaemon
{
    public static class DependencyInjectionSetup
    {
        public static IHostBuilder UseNetDaemonHostSingleton(this IHostBuilder hostBuilder)
        {
            if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));

            return hostBuilder.ConfigureServices((_, services) => { AddNetDaemonServices(services); });
        }

        public static IServiceCollection AddNetDaemonServices(this IServiceCollection services)
        {
            // replace the transient IHassClient with the one that is setup by NetDaemonHost
            services.AddTransient(s => s.GetRequiredService<NetDaemonHost>().Client);

            services.AddSingleton<NetDaemonHost>();
            services.AddSingleton<INetDaemonHost>(s => s.GetRequiredService<NetDaemonHost>());
            services.AddSingleton<INetDaemon>(s => s.GetRequiredService<NetDaemonHost>());

            // Provide services created by NetDaemonHost as separate services so apps only need to take a dependency on 
            // these interfaces and methods directly on INetDaemonHost can eventually be removed
            services.AddSingleton(s => s.GetRequiredService<NetDaemonHost>().HassEventsObservable);
            services.AddSingleton<ITextToSpeechService>(s => s.GetRequiredService<NetDaemonHost>().TextToSpeechService);
            services.AddNetDaemonScheduler();
            services.AddNetDaemonAppServices();

            return services;
        }
    }
}