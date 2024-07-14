using Microsoft.Extensions.Configuration;

namespace NetDaemon.AppModel.Internal.Config;

/// <summary>
/// Wrapper around the ConfigurationBinder to make it available from the service provider
/// and to inject the IServiceProvider to allow instancing objects that are registered services
/// </summary>
internal class ConfigurationBinding(IServiceProvider provider) : IConfigurationBinding
{
    private readonly IServiceProvider _provider = provider;

    public T? ToObject<T>(IConfiguration configuration)
    {
        return configuration.Get<T>(_provider);
    }
}
