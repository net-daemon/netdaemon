using Microsoft.Extensions.DependencyInjection;

namespace NetDaemon.DI
{
    /// <summary>
    ///     Interface for objects implementing the InstanceDaemonApps features
    /// </summary>
    public interface IInstanceDaemonAppServiceConfigurator
    {
        /// <summary>
        ///     Number of instanced daemonappServices
        /// </summary>
        int Count { get; }

        /// <summary>
        ///     Returns a list of instanced daemonapps
        /// </summary>
        IServiceCollection ConfigureServices(IServiceCollection services);
    }
}