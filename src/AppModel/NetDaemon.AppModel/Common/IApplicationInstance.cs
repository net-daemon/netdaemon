namespace NetDaemon.AppModel.Common;

/// <summary>
///     Provides metadata for a NetDaemon Application
/// </summary>
public interface IApplicationInstance
{
    /// <summary>
    ///     Unique id of the application
    /// </summary>
    string? Id { get; }
}
