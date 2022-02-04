using System.Text.Json;
using Microsoft.Extensions.Options;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Manage entities via MQTT
/// </summary>
public class MqttEntityManager : IMqttEntityManager
{
    private readonly IMessageSender    _messageSender;
    private readonly MqttConfiguration _config;

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
    public async Task CreateAsync(string domain, string deviceClass, string entityId, string name)
    {
        var payload = JsonSerializer.Serialize(new
        {
            name                  = name,
            device_class          = deviceClass,
            state_topic           = StatePath(_config.DiscoveryPrefix, domain, entityId),
            json_attributes_topic = AttrsPath(_config.DiscoveryPrefix, domain, entityId)
        });
        await _messageSender.SendMessageAsync(ConfigPath(_config.DiscoveryPrefix, domain, entityId), payload).ConfigureAwait(false);
    }

    /// <summary>
    /// Update state and, optionally, attributes of an HA entity via MQTT
    /// </summary>
    /// <param name="domain"></param>
    /// <param name="entityId"></param>
    /// <param name="state"></param>
    /// <param name="attributes">Json string of attributes</param>
    public async Task UpdateAsync(string domain, string entityId, string state, string? attributes = null)
    {
        await _messageSender.SendMessageAsync(StatePath(_config.DiscoveryPrefix, domain, entityId), state).ConfigureAwait(false);
        if (attributes != null)
            await _messageSender.SendMessageAsync(AttrsPath(_config.DiscoveryPrefix, domain, entityId), attributes).ConfigureAwait(false);
    }

    /// <summary>
    /// Remove an entity from Home Assistant
    /// </summary>
    /// <param name="domain"></param>
    /// <param name="entityId"></param>
    public async Task RemoveAsync(string domain, string entityId)
    {
        await _messageSender.SendMessageAsync(ConfigPath(_config.DiscoveryPrefix, domain, entityId), string.Empty).ConfigureAwait(false);
    }

    private static string AttrsPath(string discoveryPrefix, string domain, string entityId)
    {
        return $"{RootPath(discoveryPrefix, domain, entityId)}/attributes";
    }

    private static string StatePath(string discoveryPrefix, string domain, string entityId)
    {
        return $"{RootPath(discoveryPrefix, domain, entityId)}/state";
    }

    private static string ConfigPath(string discoveryPrefix, string domain, string entityId)
    {
        return $"{RootPath(discoveryPrefix, domain, entityId)}/config";
    }

    private static string RootPath(string discoveryPrefix, string domain, string entityId)
    {
        return $"{discoveryPrefix}/{domain}/{entityId}";
    }
}