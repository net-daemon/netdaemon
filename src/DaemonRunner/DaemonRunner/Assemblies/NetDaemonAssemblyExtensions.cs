using System;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetDaemon.Common.Configuration;
using NetDaemon.Daemon;
using NetDaemon.DaemonHost;
using NetDaemon.Service.App;

namespace NetDaemon.Assemblies;

public static class NetDaemonAssemblyExtensions
{
    public static IHostBuilder UseNetDaemonAssemblyCompiler(this IHostBuilder hostBuilder)
    {
        if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));

        hostBuilder.EnsureIsDaemonHostBuilder();

        hostBuilder.AddNetDaemonFeature(services =>
        {
            ILoggerFactory loggerFactory = new NullLoggerFactory();
            ILogger<DaemonAssemblyCompiler> logger = loggerFactory.CreateLogger<DaemonAssemblyCompiler>();
            IDaemonAssemblyCompiler compiler;

            var settings = services.GetServiceFromCollection<NetDaemonSettings>();

            if (UseLocalAssemblyLoading())
                compiler = new LocalDaemonAssemblyCompiler(logger);
            else
                compiler = new DaemonAssemblyCompiler(logger, settings);

            var manager = services.GetNetDaemonAssemblyManager();
            manager.Load(compiler);

            services.AddSingleton<INetDaemonAssemblies>(new NetDaemonAssemblyService(manager.LoadedAssemblies));
        });
                
        return hostBuilder;
    }
    
    /// <summary>
    ///     Returns true if local loading of assemblies should be preferred.
    ///     This is typically when running in container. When running in dev
    ///     you want the local loading
    /// </summary>
    private static bool UseLocalAssemblyLoading()
    {
        var appSource = Environment.GetEnvironmentVariable("NETDAEMON__APPSOURCE");

        if (string.IsNullOrEmpty(appSource))
            return true;

        return appSource.EndsWith(".csproj", true, CultureInfo.InvariantCulture)
               || appSource.EndsWith(".dll", true, CultureInfo.InvariantCulture);
    }

    public static INetDaemonAssemblyManager GetNetDaemonAssemblyManager(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        
        var manager = services.GetServiceFromCollectionOrRegisterDefault<INetDaemonAssemblyManager>(() => new NetDaemonAssemblyManager());

        return manager;
    }

}