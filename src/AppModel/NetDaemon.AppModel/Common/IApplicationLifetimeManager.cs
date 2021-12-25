namespace NetDaemon.AppModel.Common;

/// <summary>
///     Handle loaded applications
/// </summary>
/// <remarks>
///     This interface manage the life cykle of loaded applications
/// </remarks>
public interface IApplicationLifetimeManager
{
    /// <summary>
    ///     Returns all loaded applications
    /// </summary>
    IReadOnlyCollection<IApplicationInstance> Applications { get; }

    //
}
