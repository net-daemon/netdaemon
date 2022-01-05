namespace NetDaemon.AppModel;

public interface IAppModel : IAsyncDisposable
{
    /// <summary>
    ///     Instance and configure all applications.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <remark>
    ///     All apps that are configured with [NetDaemonApp] attribute or in yaml will be handled.
    ///     [Focus] attribute will only load the apps that has this attribute if exist
    ///     Depending on the selected compilation type it will be local or dynamically compiled apps
    /// </remark>
    /// <returns></returns>
    Task<IAppModelContext> InitializeAsync(CancellationToken cancellationToken);
}