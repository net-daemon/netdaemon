using System.IO;
using System.Net;
using JoySoftware.HomeAssistant.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NetDaemon.Common.Configuration;
using NetDaemon.Daemon;
using NetDaemon.Daemon.Storage;
using NetDaemon.Service;

namespace NetDaemon
{
    public static class NetDaemonExtensions
    {
        public static IHostBuilder UseNetDaemon(this IHostBuilder hostBuilder)
        {

            return hostBuilder.ConfigureServices((context, services) =>
            {
                services.Configure<HomeAssistantSettings>(context.Configuration.GetSection("HomeAssistant"));
                services.Configure<NetDaemonSettings>(context.Configuration.GetSection("NetDaemon"));
            }).ConfigureWebHostDefaults(webbuilder =>
            {
                webbuilder.UseKestrel(options =>
                {
                });
                webbuilder.UseStartup<ApiStartup>();
            });
        }
    }
}