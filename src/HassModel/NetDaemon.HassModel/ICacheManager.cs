namespace NetDaemon.HassModel;

/// <summary>
/// Allows initialization and refreshing of the HassModel internal caches
/// </summary>
public interface ICacheManager
{
    /// <summary>
    /// (re) Initializes the HassModel internal caches from Home Assistant. Should be called after (re)connecting
    /// </summary>
    Task InitializeAsync(IHomeAssistantConnection homeAssistantConnection, CancellationToken cancellationToken);
}
