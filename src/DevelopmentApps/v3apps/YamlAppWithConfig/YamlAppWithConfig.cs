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
        ILogger<AppWithYamlConfig> logger,
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
        logger.LogInformation("{who} cooks today!", _config?.WhoCooks?.State);
    }
}

public record Notification
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
public record MyConfig
{
    public InputSelectEntity? WhoCooks { get; set; }
    public int SomeInt { get; set; }
    public IEnumerable<Notification>? Notifications { get; set; }
}

public record InputSelectEntity : NetDaemon.HassModel.Entities.Entity<InputSelectEntity, NetDaemon.HassModel.Entities.EntityState<InputSelectAttributes>, InputSelectAttributes>
{
    public InputSelectEntity(NetDaemon.HassModel.Common.IHaContext haContext, string entityId) : base(haContext, entityId)
    {
    }

    public InputSelectEntity(NetDaemon.HassModel.Entities.Entity entity) : base(entity)
    {
    }
}

public record InputSelectAttributes
{
    [System.Text.Json.Serialization.JsonPropertyNameAttribute("editable")]
    public bool? Editable { get; init; }

    [System.Text.Json.Serialization.JsonPropertyNameAttribute("friendly_name")]
    public string? FriendlyName { get; init; }

    [System.Text.Json.Serialization.JsonPropertyNameAttribute("icon")]
    public string? Icon { get; init; }

    [System.Text.Json.Serialization.JsonPropertyNameAttribute("options")]
    public object? Options { get; init; }
}