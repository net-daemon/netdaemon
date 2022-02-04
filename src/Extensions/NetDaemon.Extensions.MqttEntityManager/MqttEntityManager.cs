#region

using System.Text.Json;
using Microsoft.Extensions.Options;

#endregion

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Manage entities via MQTT
/// </summary>
public class MqttEntityManager : IMqttEntityManager
{
    private readonly MqttConfiguration _config;
    private readonly IMessageSender    _messageSender;

    /// <summary>
    /// Manage entities via MQTT
    /// </summary>
    /// <param name="messageSender"></param>
    /// <param name="config"></param>
    public MqttEntityManager(IMessageSender messageSender, IOptions<MqttConfiguration> config)
    {
        _messageSender = messageSender;
        _config        = config.Value;
    }

    /// <summary>   
    /// Create an entity in Home Assistant via MQTT
    /// </summary>
    /// <param name="domain"></param>
    /// <param name="deviceClass"></param>
    /// <param name="entityId"></param>
    /// <param name="name"></param>
     public async Task CreateAsync(string domain, string entityId, string deviceClass, string name)
    {
        var payload = JsonSerializer.Serialize(new
        {
            name,
            device_class          = deviceClass,
            state_topic           = StatePath(domain, entityId),
            json_attributes_topic = AttrsPath(domain, entityId)
        });
        await _messageSender.SendMessageAsync(ConfigPath(domain, entityId), payload).ConfigureAwait(false);
    }

    /// <summary>
    /// Update state and, optionally, attributes of an HA entity via MQTT
    /// </summary>
    /// <param name="domain"></param>
    /// <param name="entityId"></param>
    /// <param name="state"></param>
    /// <param name="attributes">Json string of attributes</param>
    /// <summary>
    /// Remove an entity from Home Assistant
    /// </summary>
    /// <param name="domain"></param>
    /// <param name="entityId"></param>
    public async Task RemoveAsync(string domain, string entityId)
    {
        await _messageSender.SendMessageAsync(ConfigPath(domain, entityId), string.Empty).ConfigureAwait(false);
    }

    public async Task UpdateAsync(string domain, string entityId, string state, string? attributes = null)
    {
        await _messageSender.SendMessageAsync(StatePath(domain, entityId), state).ConfigureAwait(false);
        if (attributes != null)
            await _messageSender.SendMessageAsync(AttrsPath(domain, entityId), attributes).ConfigureAwait(false);
    }

    private string AttrsPath(string domain, string entityId) => $"{RootPath(domain, entityId)}/attributes";

    private string ConfigPath(string domain, string entityId) => $"{RootPath(domain, entityId)}/config";

    private string RootPath(string domain, string entityId) => $"{_config.DiscoveryPrefix}/{domain}/{entityId}";

    private string StatePath(string domain, string entityId) => $"{RootPath(domain, entityId)}/state";
}