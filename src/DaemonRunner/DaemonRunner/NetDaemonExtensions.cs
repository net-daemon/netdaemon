using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.Common.Configuration;
using NetDaemon.Daemon.Config;
using NetDaemon.Service;
using NetDaemon.Service.App;

namespace NetDaemon
{
    public static class NetDaemonExtensions
    {
        public static IHostBuilder UseNetDaemon(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices((context, services) =>
                {
                    services.Configure<HomeAssistantSettings>(context.Configuration.GetSection("HomeAssistant"));
                    services.Configure<NetDaemonSettings>(context.Configuration.GetSection("NetDaemon"));
                    services.AddSingleton<IYamlConfig, YamlConfig>();

                    RegisterNetDaemonAssembly(services);

                })
                .ConfigureWebHostDefaults(webbuilder =>
                {
                    webbuilder.UseKestrel(options => { });
                    webbuilder.UseStartup<ApiStartup>();
                });
        }

        private static void RegisterNetDaemonAssembly(IServiceCollection services)
        {
            if (BypassLocalAssemblyLoading())
                services.AddSingleton<IDaemonAppCompiler, LocalDaemonAppCompiler>();
            else
                services.AddSingleton<IDaemonAppCompiler, DaemonAppCompiler>();
        }

        private static bool BypassLocalAssemblyLoading()
        {
            var value = Environment.GetEnvironmentVariable("HASS_DISABLE_LOCAL_ASM");
            return bool.TryParse(value, out var result) && result;
        }
    }
}