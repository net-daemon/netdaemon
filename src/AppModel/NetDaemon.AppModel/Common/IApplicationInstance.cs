namespace NetDaemon.AppModel;

/// <summary>
///     Provides metadata for a NetDaemon Application
/// </summary>
public interface IApplicationInstance : IAsyncDisposable
{
    /// <summary>
    ///     Unique id of the application
    /// </summary>
    string? Id { get; }
}