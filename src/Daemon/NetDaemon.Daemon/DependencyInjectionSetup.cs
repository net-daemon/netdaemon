using System;
using JoySoftware.HomeAssistant.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.Daemon;

namespace NetDaemon
{
    public static class DependencyInjectionSetup
    {
        public static IHostBuilder UseNetDaemonHostSingleton(this IHostBuilder hostBuilder)
        {
            if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));

            return hostBuilder.ConfigureServices((_, services) =>
            {
                // replace the transient IHassClient with a singleton
                services.AddSingleton<IHassClient, HassClient>();

                services.AddSingleton<NetDaemonHost>();
                services.AddSingleton<INetDaemonHost>(s => s.GetRequiredService<NetDaemonHost>());

                // Provide services created by NetDaemonHost as separate services so apps only need to take a dependency on 
                // these interfaces and methods directly on INetDaemonHost can eventually be removed
                services.AddSingleton(s => s.GetRequiredService<NetDaemonHost>().HassEventsObservable);
                services.AddSingleton<ITextToSpeechService>(s =>
                    s.GetRequiredService<NetDaemonHost>().TextToSpeechService);
            });
        }
    }
}