namespace NetDaemon.HassModel;

/// <summary>
/// Allows initialization and refreshing of the HassModel internal caches
/// </summary>
public interface ICacheManager
{
    /// <summary>
    /// (re) Initializes the Hass Model internal caches from Home Assistant. Should be called
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task InitializeAsync(CancellationToken cancellationToken);
}