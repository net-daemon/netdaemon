namespace NetDaemon.Client;

/// <summary>
/// Interface to retrieve the current connection to HomeAsstant
/// </summary>
public interface IHomeAssistantConnectionProvider
{
    /// <summary>
    /// The current connection to Home Assistant. Null if disconnected.
    /// </summary>
    IHomeAssistantConnection? CurrentConnection { get; }
}
