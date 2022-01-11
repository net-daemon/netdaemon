namespace NetDaemon.AppModel.Internal;

internal enum AppLoadMode
{
    /// <summary>
    ///     The app will never load
    /// </summary>
    AlwaysDisabled,
    /// <summary>
    ///     The app will always load
    /// </summary>
    AlwaysEnabled,
    /// <summary>
    ///     The app will load depending on state manager result
    /// </summary>
    UseStateManager
}