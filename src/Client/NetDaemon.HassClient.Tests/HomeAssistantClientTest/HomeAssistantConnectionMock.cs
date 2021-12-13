namespace NetDaemon.HassClient.Tests.HomeAssistantClientTest;

internal class HomeAssistantConnectionMock : Mock<IHomeAssistantConnection>
{
    private readonly Channel<HassConfig> _responseConfigMessageChannel = Channel.CreateBounded<HassConfig>(10);

    internal void AddConfigResponseMessage(HassConfig config) => _responseConfigMessageChannel.Writer.TryWrite(config);
    public HomeAssistantConnectionMock()
    {
        Setup(n => n.SendCommandAndReturnResponseAsync<SimpleCommand, HassConfig>(
            It.IsAny<SimpleCommand>(), It.IsAny<CancellationToken>())).Returns(
            async (SimpleCommand _, CancellationToken _) => await _responseConfigMessageChannel.Reader.ReadAsync(CancellationToken.None));
    }
}
