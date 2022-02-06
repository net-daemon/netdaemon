#region

using System.Text.Json;
using Microsoft.Extensions.Options;
using NetDaemon.Extensions.MqttEntityManager.Helpers;
using NetDaemon.Extensions.MqttEntityManager.Models;

#endregion

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
///     Manage entities via MQTT
/// </summary>
internal class MqttEntityManager : IMqttEntityManager
{
    private readonly MqttConfiguration _config;
    private readonly IMessageSender _messageSender;

    /// <summary>
    ///     Manage entities via MQTT
    /// </summary>
    /// <param name="messageSender"></param>
    /// <param name="config"></param>
    public MqttEntityManager(IMessageSender messageSender, IOptions<MqttConfiguration> config)
    {
        _messageSender = messageSender;
        _config = config.Value;
    }

    /// <summary>
    /// Create an entity in Home Assistant via MQTT
    /// </summary>
    /// <param name="entityId">Distinct identifier, in the format "domain.id", such as "sensor.kitchen_temp"</param>
    /// <param name="options">Optional set of additional parameters</param>
    public async Task CreateAsync(string entityId, EntityCreationOptions? options)
    {
        var (domain, identifier) = EntityIdParser.Extract(entityId);
        var configPath = ConfigPath(domain, identifier);

        var payload = JsonSerializer.Serialize(new EntityCreationPayload
            {
                Name = options?.Name ?? identifier,
                DeviceClass = options?.DeviceClass,
                UniqueId = options?.UniqueId ?? configPath.Replace('/', '_'),
                CommandTopic = CommandPath(domain, identifier),
                StateTopic = StatePath(domain, identifier),
                JsonAttributesTopic = AttrsPath(domain, identifier)
            }
        );

        await _messageSender
            .SendMessageAsync(configPath, payload, options?.Persist ?? true)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Remove an entity from Home Assistant
    /// </summary>
    /// <param name="entityId"></param>
    public async Task RemoveAsync(string entityId)
    {
        var (domain, identifier) = EntityIdParser.Extract(entityId);
        await _messageSender.SendMessageAsync(ConfigPath(domain, identifier), string.Empty).ConfigureAwait(false);
    }

    /// <summary>
    ///     Update state and, optionally, attributes of an HA entity via MQTT
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="state">New state</param>
    /// <param name="attributes">Concrete or anonymous attributes</param>
    public async Task UpdateAsync(string entityId, string? state, object? attributes = null)
    {
        var (domain, identifier) = EntityIdParser.Extract(entityId);
        
        if (!string.IsNullOrWhiteSpace(state))
            await _messageSender.SendMessageAsync(StatePath(domain, identifier), state).ConfigureAwait(false);
        
        if (attributes != null)
            await _messageSender.SendMessageAsync(AttrsPath(domain, identifier), JsonSerializer.Serialize(attributes) ).ConfigureAwait(false);
    }

    private string AttrsPath(string domain, string identifier) => $"{RootPath(domain, identifier)}/attributes";

    private string ConfigPath(string domain, string identifier) => $"{RootPath(domain, identifier)}/config";

    private string RootPath(string domain, string identifier) => $"{_config.DiscoveryPrefix}/{domain}/{identifier}";

    private string StatePath(string domain, string identifier) => $"{RootPath(domain, identifier)}/state";

    private string CommandPath(string domain, string identifier) => $"{RootPath(domain, identifier)}/set";
}