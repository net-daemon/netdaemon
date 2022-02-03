namespace NetDaemon.Extensions.MqttEntities;

public interface IEntityUpdater
{
    Task CreateAsync(string deviceType, string deviceClass, string entityId, string name);
}