using Microsoft.Extensions.Configuration;

namespace NetDaemon.AppModel.Internal.Config;

/// <summary>
/// Interface for configuration binding to object
/// </summary>
internal interface IConfigurationBinding
{
    T? ToObject<T>(IConfiguration configuration);
}
