namespace NetDaemon.Extensions.MqttEntityManager;

public interface IMqttEntityManager
{
    Task CreateAsync(string domain, string deviceClass, string entityId, string name);
    Task UpdateAsync(string domain, string entityId, string state, string? attributes);
    Task RemoveAsync(string domain, string entityId);
}