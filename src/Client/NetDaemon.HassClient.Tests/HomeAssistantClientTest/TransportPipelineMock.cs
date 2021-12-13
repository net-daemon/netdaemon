namespace NetDaemon.HassClient.Tests.HomeAssistantClientTest;

internal class TransportPipelineMock : Mock<IWebSocketClientTransportPipeline>
{
    private readonly Channel<HassMessage> _responseMessageChannel = Channel.CreateBounded<HassMessage>(100);

    public void AddResponse(HassMessage msg) => _responseMessageChannel.Writer.TryWrite(msg);
    public TransportPipelineMock()
    {
        Setup(n => n.GetNextMessageAsync<HassMessage>(It.IsAny<CancellationToken>())).Returns(
            async (CancellationToken _) => await _responseMessageChannel.Reader.ReadAsync(CancellationToken.None).ConfigureAwait(false));
    }
}
