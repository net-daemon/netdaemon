using System.ComponentModel;
using System.Globalization;
using NetDaemon.AppModel.Common;
using NetDaemon.AppModel.Tests.Config;

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
    string IAmInjected { get; }
}
public class InjectMeImplementation : IInjectMePlease
{
    public string IAmInjected => "ok I am hard coded, so what?";
}

public class EntityClass
{
    private readonly IServiceProvider _serviceProvider;

    public EntityClass(
        IServiceProvider serviceProvider,
        string entityId
    )
    {
        _serviceProvider = serviceProvider;
        EntityId = entityId;
    }

    public string EntityId { get; }
}
