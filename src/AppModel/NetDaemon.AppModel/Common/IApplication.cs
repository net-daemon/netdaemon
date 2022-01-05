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
    ///     Current state of the application
    /// </summary>
    ApplicationState State { get; }
}