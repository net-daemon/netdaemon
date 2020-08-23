using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.Common.Configuration;
using NetDaemon.Daemon.Config;
using NetDaemon.Service;

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
                })
                .ConfigureWebHostDefaults(webbuilder =>
                {
                    webbuilder.UseKestrel(options => { });
                    webbuilder.UseStartup<ApiStartup>();
                });
        }
    }
}