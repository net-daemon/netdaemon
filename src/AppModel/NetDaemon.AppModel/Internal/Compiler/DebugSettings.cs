namespace NetDaemon.AppModel.Internal.Compiler;

/// <summary>
///     Used to set debug or release mode for dynamic compiled apps
/// </summary>
internal record DebugSettings
{
    public bool UseDebug { get; set; } = false;
}
