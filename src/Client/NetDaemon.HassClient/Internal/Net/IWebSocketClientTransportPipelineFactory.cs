namespace NetDaemon.Client.Internal.Net;

internal interface IWebSocketClientTransportPipelineFactory
{
    IWebSocketClientTransportPipeline New(IWebSocketClient webSocketClient);
}