namespace NetDaemon.HassClient.Tests.HomeAssistantClientTest;

internal sealed class TransportPipelineMock : Mock<IWebSocketClientTransportPipeline>
{
    private readonly Channel<HassMessage> _responseMessageChannel = Channel.CreateBounded<HassMessage>(100);
    private readonly Channel<HassAuthResponse> _authResponseMessageChannel = Channel.CreateBounded<HassAuthResponse>(2);

    public TransportPipelineMock()
    {
        Setup(n => n.GetNextMessagesAsync<HassMessage>(It.IsAny<CancellationToken>())).Returns(
            async (CancellationToken _) =>
            {

                var msg = await _responseMessageChannel.Reader.ReadAsync(CancellationToken.None).ConfigureAwait(false);
                return new HassMessage[] { msg };
            });

        Setup(n => n.GetNextMessagesAsync<HassAuthResponse>(It.IsAny<CancellationToken>())).Returns(
            async (CancellationToken _) =>
            {

                var msg = await _authResponseMessageChannel.Reader.ReadAsync(CancellationToken.None).ConfigureAwait(false);
                return new [] { msg };
            });
    }

    public void AddResponse(HassMessage msg)
    {
        _responseMessageChannel.Writer.TryWrite(msg);
    }

    public void AddAuthResponse(HassAuthResponse msg)
    {
        _authResponseMessageChannel.Writer.TryWrite(msg);
    }
}
