using Microsoft.Extensions.Configuration;

namespace NetDaemon.AppModel.Internal.Config;

internal class AppConfig<T> : IAppConfig<T> where T : class, new()
{
    public T Value { get; }
    public AppConfig(IConfiguration config, IConfigurationBinding configBinder)
    {
        var type = typeof(T);
        var section = config.GetSection(type.FullName);
        Value = configBinder.ToObject<T>(section) ?? new T();
    }
}