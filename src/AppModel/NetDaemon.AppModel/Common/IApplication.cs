namespace NetDaemon.AppModel;

/// <summary>
///     Represents a application and it's state
/// </summary>
public interface IApplication : IAsyncDisposable
{
    /// <summary>
    ///     Unique id of the application
    /// </summary>
    string? Id { get; }

    /// <summary>
    /// Indicates if this application is currently Enabled
    /// </summary>
    public bool Enabled { get; }

    /// <summary>
    /// Enables the App and loads if possible
    /// </summary>
    public Task EnableAsync();

    /// <summary>
    /// Disable the app and unload (Dispose) it if it is running
    /// </summary>
    public Task DisableAsync();

    /// <summary>
    /// The currently running instance of the app (if any)
    /// </summary>
    public object? Instance { get; }

}