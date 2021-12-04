using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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

        hostBuilder.ConfigureServices(services =>
        {
            if (UseLocalAssemblyLoading())
                services.AddSingleton<IDaemonAssemblyCompiler, LocalDaemonAssemblyCompiler>();
            else
                services.AddSingleton<IDaemonAssemblyCompiler, DaemonAssemblyCompiler>();
        });

        hostBuilder.AddNetDaemonFeature((context, services) =>
        {
            IDaemonAssemblyCompiler compiler = context.ServiceProvider.GetRequiredService<IDaemonAssemblyCompiler>();

            var manager = services.GetNetDaemonAssemblyManager();
            var assemblies = manager.Load(compiler, context.ServiceProvider).ToList();

            services.AddSingleton<INetDaemonAssemblies>(new NetDaemonAssemblyService(assemblies));
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
    
    public static IServiceCollection ConfigureNetDaemonAssemblies(this IServiceCollection services, Action<List<Assembly>, IServiceProvider> configure)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        
        var assembliesManager = services.GetNetDaemonAssemblyManager();
        assembliesManager.ConfigureAssemblies(configure);
        
        return services;
    }

}