namespace NetDaemon.AppModel;

/// <summary>
///     The current state of an application
/// </summary>
public enum ApplicationState
{
    /// <summary>
    ///     Application is enabled
    /// </summary>
    Enabled,
    /// <summary>
    ///     Application is disabled
    /// </summary>
    Disabled,
    /// <summary>
    ///     The application is in running state
    /// </summary>
    Running,
    /// <summary>
    ///     Application is in error state
    /// </summary>
    Error
}
