#region

using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MQTTnet.Protocol;
using NetDaemon.Extensions.MqttEntityManager.Helpers;
using NetDaemon.Extensions.MqttEntityManager.Models;

#endregion

[assembly: InternalsVisibleTo("NetDaemon.HassClient.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
///     Manage entities via MQTT
/// </summary>
internal class MqttEntityManager : IMqttEntityManager
{
    private readonly MqttConfiguration _config;
    private readonly IMessageSender    _messageSender;
    private readonly IMessageReceiver  _messageReceiver;

    public MqttQualityOfServiceLevel QualityOfServiceLevel { get; set; } = MqttQualityOfServiceLevel.AtMostOnce;

    /// <summary>
    ///     Manage entities via MQTT
    /// </summary>
    /// <param name="messageSender"></param>
    /// <param name="messageReceiver"></param>
    /// <param name="config"></param>
    public MqttEntityManager(IMessageSender messageSender, IMessageReceiver messageReceiver, IOptions<MqttConfiguration> config)
    {
        _messageSender = messageSender;
        _messageReceiver = messageReceiver;
        _config = config.Value;
    }

    /// <summary>
    /// Create an entity in Home Assistant via MQTT
    /// </summary>
    /// <param name="entityId">Distinct identifier, in the format "domain.id", such as "sensor.kitchen_temp"</param>
    /// <param name="options">Optional set of additional parameters</param>
    /// <param name="additionalConfig"></param>
    public async Task CreateAsync(string entityId, EntityCreationOptions? options = null,
        object? additionalConfig = null)
    {
        var (domain, identifier) = EntityIdParser.Extract(entityId);
        var configPath = ConfigPath(domain, identifier);

        var payload = BuildCreationPayload(domain, identifier, configPath, options, additionalConfig);

        await _messageSender
            .SendMessageAsync(configPath, payload, options?.Persist ?? true, QualityOfServiceLevel)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Remove an entity from Home Assistant
    /// </summary>
    /// <param name="entityId"></param>
    public async Task RemoveAsync(string entityId)
    {
        var (domain, identifier) = EntityIdParser.Extract(entityId);
        await _messageSender
            .SendMessageAsync(ConfigPath(domain, identifier), string.Empty, false, QualityOfServiceLevel)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Set the state of an entity
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task SetStateAsync(string entityId, string state)
    {
        var (domain, identifier) = EntityIdParser.Extract(entityId);

        await _messageSender.SendMessageAsync(StatePath(domain, identifier), state, true, QualityOfServiceLevel)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Set attributes on an entity
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="attributes"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task SetAttributesAsync(string entityId, object attributes)
    {
        var (domain, identifier) = EntityIdParser.Extract(entityId);

        await _messageSender.SendMessageAsync(AttrsPath(domain, identifier), JsonSerializer.Serialize(attributes),
                true, QualityOfServiceLevel)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Set availability of the entity. If you specified "payload_available" and "payload_not_available" configuration
    /// on creating the entity then the value should match one of these.
    /// If not, then use "online" and "offline"
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="availability"></param>
    public async Task SetAvailabilityAsync(string entityId, string availability)
    {
        var (domain, identifier) = EntityIdParser.Extract(entityId);

        await _messageSender
            .SendMessageAsync(AvailabilityPath(domain, identifier), availability, true, QualityOfServiceLevel)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Subscribe to a topic
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IObservable<string>> SubscribeEntityCommandAsync(string entityId)
    {
        var (domain, identifier) = EntityIdParser.Extract(entityId);
        return await _messageReceiver.SubscribeTopicAsync(CommandPath(domain, identifier)).ConfigureAwait(false);
    }

    private string BuildCreationPayload(string domain, string identifier, string configPath,
        EntityCreationOptions? options, object? additionalConfig)
    {
        var availabilityRequired = IsAvailabilityTopicRequired(options);

        var concreteOptions = new EntityCreationPayload
        {
            Name = options?.Name ?? identifier,
            DeviceClass = options?.DeviceClass,
            UniqueId = options?.UniqueId ?? configPath.Replace('/', '_'),
            CommandTopic = CommandPath(domain, identifier),
            StateTopic = StatePath(domain, identifier),
            PayloadAvailable = options?.PayloadAvailable,
            PayloadNotAvailable = options?.PayloadNotAvailable,
            PayloadOn = options?.PayloadOn,
            PayloadOff = options?.PayloadOff,
            AvailabilityTopic = availabilityRequired ? AvailabilityPath(domain, identifier) : null,
            JsonAttributesTopic = AttrsPath(domain, identifier)
        };

        return EntityCreationPayloadHelper.Merge(concreteOptions, additionalConfig);
    }

    private bool IsAvailabilityTopicRequired(EntityCreationOptions? options)
    {
        return options != null && (!string.IsNullOrWhiteSpace(options.PayloadAvailable) ||
                                   !string.IsNullOrWhiteSpace(options.PayloadNotAvailable));
    }

    private string AttrsPath(string domain, string identifier) => $"{RootPath(domain, identifier)}/attributes";

    private string ConfigPath(string domain, string identifier) => $"{RootPath(domain, identifier)}/config";

    private string RootPath(string domain, string identifier) => $"{_config.DiscoveryPrefix}/{domain}/{identifier}";

    private string StatePath(string domain, string identifier) => $"{RootPath(domain, identifier)}/state";

    private string CommandPath(string domain, string identifier) => $"{RootPath(domain, identifier)}/set";

    private string AvailabilityPath(string domain, string identifier) => $"{RootPath(domain, identifier)}/availability";
}