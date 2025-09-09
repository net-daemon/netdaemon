using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.Extensions.Logging.Internal;
using Serilog;

namespace NetDaemon.Extensions.Logging;

/// <summary>
///     Adds extension to the IHostBuilder to add default NetDaemon logging capabilities
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

    /// <summary>
    ///    Adds default logging capabilities for NetDaemon
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AddNetDaemonDefaultLogging(this IServiceCollection services)
    {
        services.AddSerilog((context, loggerConfiguration) =>
            SerilogConfigurator.Configure(loggerConfiguration, context.GetRequiredService<IHostEnvironment>()));
        return services;
    }
}
