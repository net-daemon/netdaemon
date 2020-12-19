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
            if (!BypassLocalAssemblyLoading())
                services.AddSingleton<IDaemonAppCompiler, LocalDaemonAppCompiler>();
            else
                services.AddSingleton<IDaemonAppCompiler, DaemonAppCompiler>();
        }

        /// <summary>
        ///     Returns true if local loading of assemblies should be preferred. 
        ///     This is typically when running in container. When running in dev
        ///     you want the local loading
        /// </summary>
        private static bool BypassLocalAssemblyLoading()
        {

            var appSource = Environment.GetEnvironmentVariable("NETDAEMON__APPSOURCE") ??
                throw new NullReferenceException("NETDAEMON__APPSOURCE cannot be null!");

            if (appSource.EndsWith(".csproj") || appSource.EndsWith(".dll"))
                return false;
            else
                return true;
        }
    }
}