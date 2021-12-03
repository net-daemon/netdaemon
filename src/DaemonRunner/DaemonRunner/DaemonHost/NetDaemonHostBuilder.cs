using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NetDaemon.DaemonHost;

public class NetDaemonHostBuilder : IHostBuilder
{
    private IHostBuilder _hostBuilderImplementation;

    public NetDaemonHostBuilder(IHostBuilder hostBuilderImplementation)
    {
        _hostBuilderImplementation = hostBuilderImplementation;

        UseServiceProviderFactory(new DefaultServiceProviderFactory());
    }

    public IHost Build()
    {
        return _hostBuilderImplementation.Build();
    }

    public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
    {
        _hostBuilderImplementation.ConfigureAppConfiguration(configureDelegate);

        return this;
    }

    public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
    {
        _hostBuilderImplementation.ConfigureContainer(configureDelegate);

        return this;
    }

    public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
    {
        _hostBuilderImplementation.ConfigureHostConfiguration(configureDelegate);

        return this;
    }

    public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
    {
        _hostBuilderImplementation.ConfigureServices(configureDelegate);

        return this;
    }

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
    {
        _hostBuilderImplementation.UseServiceProviderFactory(new NetDaemonFeatureServiceProviderFactoryWrapper<TContainerBuilder>(factory));

        return this;
    }

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
    {
        _hostBuilderImplementation.UseServiceProviderFactory(factory);

        return this;
    }

    public IDictionary<object, object> Properties => _hostBuilderImplementation.Properties;
}