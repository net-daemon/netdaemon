namespace NetDaemon.Client.Internal;
internal interface IHomeAssistantConnectionFactory
{
    IHomeAssistantConnection New(IWebSocketClientTransportPipeline transportPipeline);
}