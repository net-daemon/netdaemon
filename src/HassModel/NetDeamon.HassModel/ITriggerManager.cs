using NetDaemon.Client.Common.HomeAssistant.Model;

namespace NetDaemon.HassModel;

public interface ITriggerManager
{
    Task<IObservable<JsonElement>> RegisterTrigger<T>(T triggerParams) where T : TriggerBase;
}