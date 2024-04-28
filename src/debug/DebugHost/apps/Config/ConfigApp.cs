using NetDaemon.AppModel;

namespace Apps;

[NetDaemonApp]
public class ConfigApp
{
    public ConfigApp(IAppConfig<AppConfig> config)
    {
        Console.WriteLine(config?.Value);
    }
}

public record AppConfig
{
    public IList<string> Strings { get; init; } = new List<string>();
}
