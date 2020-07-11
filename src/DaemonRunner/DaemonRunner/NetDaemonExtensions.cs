using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.Service;
using NetDaemon.Service.Configuration;

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

                services.AddHttpClient();
                services.AddHostedService<RunnerService>();
            });
        }
    }
}