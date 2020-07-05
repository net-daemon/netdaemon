using Microsoft.Extensions.Hosting;
using NetDaemon.Service.Support;
using Serilog;

namespace NetDaemon.Service.Extensions
{
    public static class HostBuilderExtensions
    {
        // We preserve the static logger so that we can access it statically and early on in the application lifecycle.
        private const bool PreserveStaticLogger = true;
        
        public static IHostBuilder UseNetDaemon(this IHostBuilder builder)
        {
            return builder
                .UseNetDaemonSerilog()
                .ConfigureServices(services =>
                {
                    services.AddNetDaemon();
                });
        }

        private static IHostBuilder UseNetDaemonSerilog(this IHostBuilder builder)
        {
            return builder.UseSerilog(
                (context, configuration) => SeriLogConfigurator.Configure(configuration),
                PreserveStaticLogger
            );
        }
    }
}