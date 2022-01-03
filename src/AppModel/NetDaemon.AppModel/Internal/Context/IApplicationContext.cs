namespace NetDaemon.AppModel.Internal;

/// <summary>
///     Provides application runtime data to apps
/// </summary>
internal interface IApplicationContext : IAsyncDisposable
{
    /// <summary>
    ///     Unique id of the application instance
    /// </summary>
    string Id { get; }

    /// <summary>
    ///     Gets whether this app is enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    ///     The type of the app
    /// </summary>
    Type AppType { get; }

    /// <summary>
    ///     The type of the app
    /// </summary>
    object Instance { get; }
}