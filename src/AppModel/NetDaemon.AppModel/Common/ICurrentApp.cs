namespace NetDaemon.AppModel;

/// <summary>
///     Provides information about the app that owns the current dependency injection scope.
/// </summary>
/// <remarks>
///     Resolve this from an app's constructor, or from any service registered in the app's
///     scope, to discover which app is being instantiated - for example to select per-app
///     configuration or logging.
/// </remarks>
public interface ICurrentApp
{
    /// <summary>
    ///     Gets the unique id of the app that owns the current scope. This is the same id
    ///     NetDaemon uses elsewhere: the value of <see cref="NetDaemonAppAttribute.Id"/> when
    ///     set, otherwise the full name of the app type.
    /// </summary>
    string Id { get; }
}
