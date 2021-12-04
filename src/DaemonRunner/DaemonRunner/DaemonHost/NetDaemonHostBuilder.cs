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

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) where TContainerBuilder : notnull
    {
        _hostBuilderImplementation.UseServiceProviderFactory(CreateServiceProviderFactoryWrapper(factory));

        return this;
    }

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) where TContainerBuilder : notnull
    {
        _hostBuilderImplementation.UseServiceProviderFactory(context => CreateServiceProviderFactoryWrapper(factory.Invoke(context)));

        return this;
    }

    private static IServiceProviderFactory<TContainerBuilder> CreateServiceProviderFactoryWrapper<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) where TContainerBuilder : notnull
    {
        return new NetDaemonFeatureServiceProviderFactoryWrapper<TContainerBuilder>(factory);
    }

    public IDictionary<object, object> Properties => _hostBuilderImplementation.Properties;
}