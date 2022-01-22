namespace NetDaemon.Client.Common.Settings;

/// <summary>
///     Home Assistant related settings
/// </summary>
public class HomeAssistantSettings
{
    /// <summary>
    ///     Home Assistant address
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    ///     Home Assistant port
    /// </summary>
    public int Port { get; set; } = 8123;

    /// <summary>
    ///     Connect using ssl
    /// </summary>
    public bool Ssl { get; set; }

    /// <summary>
    ///     Token to authorize
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    ///     Set this to a hash of a certificate that will bypass error checks
    /// </summary>
    public string ByPassErrorCheckForCertificateHash { get; set; } = string.Empty;

    /// <summary>
    ///     Path to websocket API, this can be different for add-on and core
    /// </summary>
    public string WebsocketPath { get; set; } = DefaultWebSocketPath;

    internal const string DefaultWebSocketPath = "api/websocket";
}