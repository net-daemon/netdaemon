namespace NetDaemon.HassClient.Tests.Integration;

internal record TestContext : IAsyncDisposable
{
    public Mock<ILogger<HomeAssistantClient>> HomeAssistantLogger { get; init; } = new();
    public Mock<ILogger<IWebSocketClientTransportPipeline>> TransportPipelineLogger { get; init; } = new();
    public Mock<ILogger<IHomeAssistantConnection>> HomeAssistantConnectionLogger { get; init; } = new();
    public IHomeAssistantConnection HomeAssistantConnction { get; init; } = new Mock<IHomeAssistantConnection>().Object;
    public async ValueTask DisposeAsync()
    {
        await HomeAssistantConnction.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}
