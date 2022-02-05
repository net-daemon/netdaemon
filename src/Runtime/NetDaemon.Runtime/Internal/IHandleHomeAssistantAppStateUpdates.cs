using NetDaemon.AppModel;

namespace NetDaemon.Runtime.Internal;

internal interface IHandleHomeAssistantAppStateUpdates
{
    Task InitializeAsync(IHomeAssistantConnection haConnection, IAppModelContext appContext);
}
