using Microsoft.Extensions.Configuration;

namespace NetDaemon.AppModel.Internal.Config;

internal class AppConfig<T> : IAppConfig<T> where T : class, new()
{
    public AppConfig(IConfiguration config, IConfigurationBinding configBinder, ILogger<AppConfig<T>> logger)
    {
        var type = typeof(T);
        var section = config.GetSection(type.FullName);

        if (!section.Exists())
        {
            logger.LogWarning("The configuration for {Type} is not found. Please add config.", typeof(T).FullName);
            Value = new T();
        }
        else
        {
            Value = configBinder.ToObject<T>(section) ?? new T();
        }
    }

    public T Value { get; }
}
