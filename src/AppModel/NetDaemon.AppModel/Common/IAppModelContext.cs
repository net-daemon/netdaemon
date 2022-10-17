namespace NetDaemon.AppModel;

/// <summary>
///     Manage AppModel state and lifecycle
/// </summary>
public interface IAppModelContext : IAsyncInitializable, IAsyncDisposable
{
    /// <summary>
    ///     Current instantiated and running applications
    /// </summary>
    IReadOnlyCollection<IApplication> Applications { get; }
}