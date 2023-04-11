using NetDaemon.Client.Common.HomeAssistant.Model;

namespace NetDaemon.HassModel;

/// <summary>
/// Enables the creation of triggers
/// </summary>
public interface ITriggerManager
{
    IObservable<JsonElement> RegisterTrigger(object triggerParams);
}