namespace NetDaemon.AppModel.Common;
public interface IAppModel : IAsyncDisposable
{
    /// <summary>
    ///     Instance and configure all applications.
    /// </summary>
    /// <remark>
    ///     All apps that are configured with [NetDaemonApp] attribute or in yaml will be handled.
    ///     [Focus] attribute will only load the apps that has this attribute if exist
    ///     Depending on the selected compilation type it will be local or dynamically compiled apps
    /// 
    ///     If any application was previously loaded prior calling load they will be unloaded and disposed
    /// </remark>
    /// <param name="skipLoadApplicationCollection">List of application id:s that will not be loaded</param>
    IReadOnlyCollection<IApplicationInstance> LoadApplications(IReadOnlyCollection<string>? skipLoadApplicationCollection = null);

    // IReadOnlyCollection<IApplicationInstance> EnableApplication(IApplicationInstance app);
    // IReadOnlyCollection<IApplicationInstance> DisableApplication(IApplicationInstance app);
}