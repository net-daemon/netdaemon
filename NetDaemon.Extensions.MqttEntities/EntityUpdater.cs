using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace NetDaemon.Extensions.MqttEntities;

public class EntityUpdater : IEntityUpdater
{
    private readonly ILogger<EntityUpdater> _logger;
    private readonly IMessageSender _messageSender;

    public EntityUpdater(ILogger<EntityUpdater> logger, IMessageSender messageSender)
    {
        _logger = logger;
        _messageSender = messageSender;
    }

    public async Task CreateAsync(string deviceType, string deviceClass, string entityId, string name)
    {
        var rootPath = $"homeassistant/{deviceType}/{entityId}";
        var topicPath = $"{rootPath}/config";
        var statePath = $"{rootPath}/state";
        var attrsPath = $"{rootPath}/attributes";

        var payload = JsonSerializer.Serialize( new
        {
            name = name, device_class = deviceClass, state_topic = statePath, json_attributes_topic = attrsPath
        });

        await _messageSender.SendMessageAsync( topicPath, payload);
    }
}