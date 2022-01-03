namespace NetDaemon.Client.Internal.Net;

internal class WebSocketClientFactory : IWebSocketClientFactory
{
    public IWebSocketClient New()
    {
        return new WebSocketClientImpl();
    }
}