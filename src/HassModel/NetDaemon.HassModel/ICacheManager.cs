namespace NetDaemon.HassModel;

/// <summary>
/// Allows initialization and refreshing of the HassModel internal caches
/// </summary>
public interface ICacheManager
{
    /// <summary>
    /// (re) Initializes the Hass Model internal caches from Home Assistant. Should be called
    /// </summary>
    /// <returns></returns>
    Task InitializeAsync(IHomeAssistantConnection homeAssistantConnection, CancellationToken cancellationToken);
}
