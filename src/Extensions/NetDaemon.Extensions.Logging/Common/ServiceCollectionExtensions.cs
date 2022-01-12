using Microsoft.Extensions.Hosting;
using NetDaemon.Extensions.Logging.Internal;
using Serilog;

namespace NetDaemon.Extensions.Logging;

/// <summary>
///     Adds extension to the IHostbuilder to add default NetDaemon logging capabilities
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds default logging capabilities for NetDaemon
    /// </summary>
    /// <param name="builder"></param>
    public static IHostBuilder UseNetDaemonDefaultLogging(this IHostBuilder builder)
    {
        return builder.UseSerilog((context, loggerConfiguration) =>
            SerilogConfigurator.Configure(loggerConfiguration, context.HostingEnvironment));
    }
}