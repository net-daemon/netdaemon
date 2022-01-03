namespace NetDaemon.Client.Internal.Net;

/// <summary>
///     Factory for Client Websocket. Implement to use for mockups
/// </summary>
internal interface IWebSocketClientFactory
{
    IWebSocketClient New();
}