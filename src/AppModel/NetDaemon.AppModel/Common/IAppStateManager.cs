namespace NetDaemon.AppModel;

public interface IAppStateManager
{
    Task<ApplicationState> GetStateAsync(string applicationId);
    Task SaveStateAsync(string applicationId, ApplicationState state);
}