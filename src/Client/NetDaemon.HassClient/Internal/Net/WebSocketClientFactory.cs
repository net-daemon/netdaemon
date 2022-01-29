namespace NetDaemon.Client.Internal.Net;

internal class WebSocketClientFactory : IWebSocketClientFactory
{
    private readonly HomeAssistantSettings _settings;

    public WebSocketClientFactory(
        IOptions<HomeAssistantSettings> settings
        )
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings.Value;
    }
    public IWebSocketClient New()
    {
        return new WebSocketClientImpl(_settings.InsecureBypassCertificateErrors);
    }
}
