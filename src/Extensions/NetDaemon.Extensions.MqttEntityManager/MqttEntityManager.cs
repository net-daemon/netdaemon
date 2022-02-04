using System.Text.Json;

namespace NetDaemon.Extensions.MqttEntityManager;

public class MqttEntityManager : IMqttEntityManager
{
    private readonly IMessageSender _messageSender;

    public MqttEntityManager(IMessageSender messageSender)
    {
        _messageSender = messageSender;
    }

    public async Task CreateAsync(string domain, string deviceClass, string entityId, string name)
    {
        var payload = JsonSerializer.Serialize(new
        {
            name                  = name,
            device_class          = deviceClass,
            state_topic           = StatePath(domain, entityId),
            json_attributes_topic = AttrsPath(domain, entityId)
        });
        await _messageSender.SendMessageAsync(ConfigPath(domain, entityId), payload).ConfigureAwait(false);
    }

    public async Task UpdateAsync(string domain, string entityId, string state, string? attributes = null)
    {
        await _messageSender.SendMessageAsync(StatePath(domain, entityId), state).ConfigureAwait(false);
        if (attributes != null)
            await _messageSender.SendMessageAsync(AttrsPath(domain, entityId), attributes).ConfigureAwait(false);
    }

    public async Task RemoveAsync(string domain, string entityId)
    {
        await _messageSender.SendMessageAsync(ConfigPath(domain, entityId), string.Empty).ConfigureAwait(false);
    }

    private static string AttrsPath(string domain, string entityId)
    {
        return $"{RootPath(domain, entityId)}/attributes";
    }

    private static string StatePath(string domain, string entityId)
    {
        return $"{RootPath(domain, entityId)}/state";
    }

    private static string ConfigPath(string domain, string entityId)
    {
        return $"{RootPath(domain, entityId)}/config";
    }

    private static string RootPath(string domain, string entityId)
    {
        return $"homeassistant/{domain}/{entityId}";
    }
}