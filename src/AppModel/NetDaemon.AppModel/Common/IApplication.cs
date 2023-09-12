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

    // Indicates if this application is currently Enabled
    public bool Enabled { get; }

    /// <summary>
    /// Enables the App and loads if possible
    /// </summary>
    public Task EnableAsync();

    
    /// <summary>
    /// Disable the app and unload (Dispose) it if it is running
    /// </summary>
    public Task DisableAsync();
}