namespace LocalApps;

public class LocalTestSettings
{
    public string AString { get; set; } = string.Empty;
    public EntityClass? Entity { get; set; } = null;
}

[NetDaemonApp]
public class MyAppLocalApp
{
    public LocalTestSettings Settings { get; }
    public MyAppLocalApp(IAppConfig<LocalTestSettings> settings)
    {
        Settings = settings.Value;
    }
}

public interface IInjectMePlease
{
    string AmInjected { get; }
}
public class InjectMeImplementation : IInjectMePlease
{
    public string AmInjected => "ok I am hard coded, so what?";
}

public class EntityClass
{
    public IServiceProvider ServiceProvider { get; }

    public EntityClass(
        IServiceProvider serviceProvider,
        string entityId
    )
    {
        ServiceProvider = serviceProvider;
        EntityId = entityId;
    }

    public string EntityId { get; }
}
