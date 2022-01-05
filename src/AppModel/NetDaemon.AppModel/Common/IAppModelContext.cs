namespace NetDaemon.AppModel;

/// <summary>
///     Manage AppModel state and lifecykle
/// </summary>
public interface IAppModelContext : IAsyncDisposable
{
    /// <summary>
    ///     Current instanciated and running applications
    /// </summary>
    IReadOnlyCollection<IApplication> Applications { get; }
}