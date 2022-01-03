using NetDaemon.AppModel;

namespace Apps;

public class TestSettings
{
    public string AString { get; set; } = string.Empty;
}

[NetDaemonApp]
public class MyApp
{
    public IAppConfig<TestSettings> Settings { get; }
    public MyApp(IAppConfig<TestSettings> settings)
    {
        Settings = settings;
    }
}