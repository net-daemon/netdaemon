namespace NetDaemon.AppModel;

/// <summary>
///     Allows apps to initialize non-blocking async operations
/// </summary>
public interface IAsyncInitializable
{
    /// <summary>
    ///     Initialize async non-blocking async operations
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to that are canceled when application unloads</param>
    Task InitializeAsync(CancellationToken cancellationToken);
}