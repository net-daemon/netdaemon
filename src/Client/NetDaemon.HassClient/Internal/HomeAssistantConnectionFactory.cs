namespace NetDaemon.Client.Internal;

internal class HomeAssistantConnectionFactory : IHomeAssistantConnectionFactory
{
    private readonly IHomeAssistantApiManager _apiManager;
    private readonly IResultMessageHandler _resultMessageHandler;
    private readonly ILogger<IHomeAssistantConnection> _logger;

    public HomeAssistantConnectionFactory(
        ILogger<IHomeAssistantConnection> logger,
        IHomeAssistantApiManager apiManager,
        IResultMessageHandler resultMessageHandler
    )
    {
        _logger = logger;
        _apiManager = apiManager;
        _resultMessageHandler = resultMessageHandler;
    }

    public IHomeAssistantConnection New(IWebSocketClientTransportPipeline transportPipeline)
    {
        return new HomeAssistantConnection(_logger, transportPipeline, _apiManager, _resultMessageHandler);
    }
}
