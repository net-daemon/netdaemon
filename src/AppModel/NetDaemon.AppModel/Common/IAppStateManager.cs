namespace NetDaemon.AppModel;

/// <summary>
///     Let users manage state of the application with own implementation
/// </summary>
public interface IAppStateManager
{
    /// <summary>
    ///     Gets application state
    /// </summary>
    /// <param name="applicationId">The unique id of the application</param>
    Task<ApplicationState> GetStateAsync(string applicationId);
    /// <summary>
    ///     Saves application state
    /// </summary>
    /// <param name="applicationId">The unique id of the application</param>
    /// <param name="state">The application state to save</param>
    Task SaveStateAsync(string applicationId, ApplicationState state);
}