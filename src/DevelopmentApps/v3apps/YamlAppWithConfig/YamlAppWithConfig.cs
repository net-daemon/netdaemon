using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.HassModel.Common;

namespace AppModelApps;

[NetDaemonApp]
public class AppWithYamlConfig
{
    private readonly MyConfig? _config;
    public AppWithYamlConfig(
        IHaContext ha,
        IAppConfig<MyConfig> config
    )
    {
        _config = config.Value;

        foreach (var notification in _config?.Notifications ?? new List<Notification>())
        {
            ha.CallService("notify", "persistent_notification",
                data: new
                {
                    message = notification.Message,
                    title = notification.Title
                });
        }
    }
}

public record Notification
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
public record MyConfig
{
    public int SomeInt { get; set; }
    public IEnumerable<Notification>? Notifications { get; set; }
}