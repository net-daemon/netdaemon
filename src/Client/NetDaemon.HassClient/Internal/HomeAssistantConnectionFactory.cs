namespace NetDaemon.Client.Internal;

internal class HomeAssistantConnectionFactory : IHomeAssistantConnectionFactory
{
    private readonly IHomeAssistantApiManager _apiManager;
    private readonly ILogger<IHomeAssistantConnection> _logger;

    public HomeAssistantConnectionFactory(
        ILogger<IHomeAssistantConnection> logger,
        IHomeAssistantApiManager apiManager
    )
    {
        _logger = logger;
        _apiManager = apiManager;
    }

    public IHomeAssistantConnection New(IWebSocketClientTransportPipeline transportPipeline)
    {
        return new HomeAssistantConnection(_logger, transportPipeline, _apiManager);
    }
}