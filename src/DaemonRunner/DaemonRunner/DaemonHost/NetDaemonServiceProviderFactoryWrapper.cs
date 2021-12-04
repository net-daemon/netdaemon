using System;
using Microsoft.Extensions.DependencyInjection;

namespace NetDaemon.DaemonHost;

public class NetDaemonFeatureServiceProviderFactoryWrapper<T> : IServiceProviderFactory<T>
{
    private readonly IServiceProviderFactory<T> _serviceProviderFactoryImplementation;

    public NetDaemonFeatureServiceProviderFactoryWrapper(IServiceProviderFactory<T> serviceProviderFactoryImplementation)
    {
        _serviceProviderFactoryImplementation = serviceProviderFactoryImplementation;
    }

    public T CreateBuilder(IServiceCollection services)
    {
        services.BuildNetDaemonFeatures();
        return _serviceProviderFactoryImplementation.CreateBuilder(services);
    }

    public IServiceProvider CreateServiceProvider(T containerBuilder)
    {
        return _serviceProviderFactoryImplementation.CreateServiceProvider(containerBuilder);
    }
}