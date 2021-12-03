﻿using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

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
    
    public static IHostBuilder AddNetDaemonFeature(this IHostBuilder hostBuilder, Action<IServiceCollection> feature)
    {
        if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));
        
        return hostBuilder.ConfigureServices((_, services) => { AddNetDaemonFeature(services, feature); });
    }
    
    public static IServiceCollection AddNetDaemonFeature(this IServiceCollection services, Action<IServiceCollection> feature)
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

    public static void EnsureIsDaemonHostBuilder(this IHostBuilder hostBuilder)
    {
        if (hostBuilder is not NetDaemonHostBuilder)
        {
            throw new Exception("Host builder is not a NetDaemonHostBuilder");
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