namespace LocalApps;

public class LocalTestSettings
{
    public string AString { get; set; } = string.Empty;
    public EntityClass? Entity { get; set; } = null;
    public DeriviedClass? Entity2 { get; set; } = null;
}

[NetDaemonApp]
public class MyAppLocalApp
{
    public MyAppLocalApp(IAppConfig<LocalTestSettings> settings)
    {
        Settings = settings.Value;
    }

    public LocalTestSettings Settings { get; }
}

public interface IInjectMePlease
{
    string AmInjected { get; }
}

public class InjectMeImplementation : IInjectMePlease
{
    public string AmInjected => "ok I am hard coded, so what?";
}
public class DeriviedClass : EntityClass
{
    public DeriviedClass(IServiceProvider serviceProvider, string entityId) : base(serviceProvider, entityId)
    {

    }
}
public class EntityClass
{
    public EntityClass(
        IServiceProvider serviceProvider,
        string entityId
    )
    {
        ServiceProvider = serviceProvider;
        EntityId = entityId;
    }

    public IServiceProvider ServiceProvider { get; }

    public string EntityId { get; }
}