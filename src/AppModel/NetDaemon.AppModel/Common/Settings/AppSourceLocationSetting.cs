namespace NetDaemon.AppModel;

/// <summary>
///     The setting for the location of source files for applications
/// </summary>
public record AppSourceLocationSetting
{
    /// <summary>
    ///     Path to the folder where to search for application source files
    ///     when using source deployment and dynamic compilation
    /// </summary>
    public string ApplicationSourceFolder { get; set; } = string.Empty;
}