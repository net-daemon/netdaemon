namespace NetDaemon.AppModel.Internal.Compiler;

/// <summary>
///     Used to set debug or release mode for dynamic compiled apps
/// </summary>
internal record CompileSettings
{
    public bool UseDebug { get; set; } = false;
}
