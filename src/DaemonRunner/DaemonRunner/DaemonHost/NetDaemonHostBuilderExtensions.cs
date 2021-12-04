using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NetDaemon.Assemblies;
using NetDaemon.Daemon;

namespace NetDaemon.DaemonHost;

public static class NetDaemonHostBuilderExtensions
{
    public static IHostBuilder CreateDefaultNetDaemonBuilder(string[] args)
    {
        return CreateNetDaemonBuilder(Host.CreateDefaultBuilder(args));
    }
    
    public static IHostBuilder CreateNetDaemonBuilder(IHostBuilder hostBuilder)
    {
        if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));

        return new NetDaemonHostBuilder(hostBuilder);
    }
    
    public static IHostBuilder UseNetDaemonBuilder(this IHostBuilder hostBuilder)
    {       
        if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));

        return CreateNetDaemonBuilder(hostBuilder);
    }
    
    public static IHostBuilder AddNetDaemonFeature(this IHostBuilder hostBuilder, Action<INetDaemonFeatureContext, IServiceCollection> feature)
    {
        if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));
        
        return hostBuilder.ConfigureServices((_, services) => { AddNetDaemonFeature(services, feature); });
    }
    
    public static IServiceCollection AddNetDaemonFeature(this IServiceCollection services, Action<INetDaemonFeatureContext, IServiceCollection> feature)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        var features = services.GetNetDaemonFeatureBuilder();

        features.AddFeature(feature);

        return services;
    }
    
    public static INetDaemonFeatureBuilder GetNetDaemonFeatureBuilder(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        
        var features = services.GetServiceFromCollectionOrRegisterDefault<INetDaemonFeatureBuilder>(() => new NetDaemonFeatureBuilder());

        return features;
    }
    
    public static void BuildNetDaemonFeatures(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        
        var featureBuilder = services.GetNetDaemonFeatureBuilder();
        featureBuilder.Build(services);
    }

    public static void BuildNetDaemonFeatures(this IHostBuilder hostBuilder)
    {
        if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));

        hostBuilder.ConfigureServices(services => services.BuildNetDaemonFeatures());
    }

    public static void EnsureIsDaemonHostBuilder(this IHostBuilder hostBuilder)
    {
        if (hostBuilder is not NetDaemonHostBuilder)
        {
            // throw new Exception("Host builder is not a NetDaemonHostBuilder");
            Console.Error.WriteLine("Host builder is not a NetDaemonHostBuilder. To use app services registration, you need to convert your host builder into a NetDaemonHostBuilder or call 'BuildNetDaemonFeatures' before building your host.");
        }
    }
    
    internal static T GetServiceFromCollectionOrRegisterDefault<T>(this IServiceCollection services, Func<T> defaultProvider)
    {
        var service = (T?)services
            .LastOrDefault(d => d.ServiceType == typeof(T))
            ?.ImplementationInstance;
        
        if (service == null)
        {
            service = defaultProvider.Invoke();
            services.TryAdd(new ServiceDescriptor(typeof(T), service));
        }

        return service;
    }
    
    internal static T? GetServiceFromCollection<T>(this IServiceCollection services)
    {
        return (T?)services
            .LastOrDefault(d => d.ServiceType == typeof(T))
            ?.ImplementationInstance;
    }
}