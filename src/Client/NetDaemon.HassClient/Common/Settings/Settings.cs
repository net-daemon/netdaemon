namespace NetDaemon.Client.Settings;

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
    ///     Set to true to ignore all certificate errors, please use at own risk
    /// </summary>
    /// <remarks>
    ///     We do not recommend to use this to bypass certificate errors.
    ///     Use other means to handle it by using valid certificates
    /// </remarks>
    public bool InsecureBypassCertificateErrors { get; set; }

    /// <summary>
    ///     Path to websocket API, this can be different for add-on and core
    /// </summary>
    public string WebsocketPath { get; set; } = DefaultWebSocketPath;

    internal const string DefaultWebSocketPath = "api/websocket";
}
