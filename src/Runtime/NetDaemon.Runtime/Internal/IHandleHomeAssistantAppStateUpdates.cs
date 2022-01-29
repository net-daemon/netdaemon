using NetDaemon.AppModel;

namespace NetDaemon.Runtime.Internal;

internal interface IHandleHomeAssistantAppStateUpdates
{
    void Initialize(IHomeAssistantConnection haConnection, IAppModelContext appContext);
}
