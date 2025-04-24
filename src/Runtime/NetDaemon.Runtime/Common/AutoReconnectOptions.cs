namespace NetDaemon.Runtime;

/// <summary>
/// Enum to determine the auto reconnect behavior of the runtime.
/// </summary>
public enum AutoReconnectOptions
{
    /// <summary>
    /// Will stop automatically reconnecting if the Home Assistant server returns an Unauthorized response. This prevents the user from being locked out of the Home Assistant server. This is the default behavior.
    /// </summary>
    StopReconnectOnUnAuthorized,

    /// <summary>
    /// Will always attempt to reconnect to the Home Assistant server.
    /// </summary>
    AlwaysAttemptReconnect
}
