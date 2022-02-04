using System.Text.Json;
using Microsoft.Extensions.Options;

namespace NetDaemon.Extensions.MqttEntityManager;

public class MqttEntityManager : IMqttEntityManager
{
    private readonly IMessageSender    _messageSender;
    private readonly MqttConfiguration _config;

    public MqttEntityManager(IMessageSender messageSender, IOptions<MqttConfiguration> config)
    {
        _messageSender = messageSender;
        _config        = config.Value;
    }

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

    public async Task UpdateAsync(string domain, string entityId, string state, string? attributes = null)
    {
        await _messageSender.SendMessageAsync(StatePath(_config.DiscoveryPrefix, domain, entityId), state).ConfigureAwait(false);
        if (attributes != null)
            await _messageSender.SendMessageAsync(AttrsPath(_config.DiscoveryPrefix, domain, entityId), attributes).ConfigureAwait(false);
    }

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