namespace NetDaemon.Client.Internal.Net;

internal class WebSocketClientTransportPipelineFactory : IWebSocketClientTransportPipelineFactory
{
    public IWebSocketClientTransportPipeline New(IWebSocketClient webSocketClient)
    {
        if (webSocketClient.State != WebSocketState.Open)
            throw new ApplicationException("Unexpected state of WebSocketClient, should be 'Open'");

        return new WebSocketClientTransportPipeline(webSocketClient);
    }
}