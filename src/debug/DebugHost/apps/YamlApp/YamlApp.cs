
using NetDaemon.AppModel;
using NetDaemon.HassModel;

namespace Apps;

[NetDaemonApp]
public class YamlApp
{
    public YamlApp(IHaContext ha, IAppConfig<TestConfig> config)
    {
        ha?.CallService("notify", "persistent_notification", data: new
        {
            message = $"My sensor is {config?.Value.MySensor}",
            title = "Hello config!"
        });
    }
}

// Todo: Make this a real HassModel entity
public record TestConfig
{
    public string? MySensor { get; set; }
}