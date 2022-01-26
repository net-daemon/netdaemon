namespace NetDaemon.AppModel;

/// <summary>
///     The setting for the location of configuration files for applications
/// </summary>
public record AppConfigurationLocationSetting
{
    /// <summary>
    ///     Path to the folder where to search for application configuration files
    /// </summary>
    public string ApplicationConfigurationFolder { get; set; } = string.Empty;
}