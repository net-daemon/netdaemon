namespace NetDaemon.HassClient.Tests.Net;

internal class WebSocketClientMock : Mock<IWebSocketClient>
{
    private readonly Channel<byte[]> _responseMessageChannel = Channel.CreateBounded<byte[]>(100);

    private byte[]? _currentMultiSpanMessage;
    private int _currentReadPosition;

    public WebSocketClientMock()
    {
        // Default Connect just returns (success)
        Setup(x => x.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Setup(x =>
                x.CloseAsync(It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                CloseStatus = WebSocketCloseStatus.NormalClosure;
                SetupGet(x => x.State).Returns(WebSocketState.Closed);
                return Task.CompletedTask;
            });

        Setup(x =>
                x.CloseOutputAsync(It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                CloseStatus = WebSocketCloseStatus.NormalClosure;
                SetupGet(x => x.State).Returns(WebSocketState.Closed);
                return Task.CompletedTask;
            });
        Setup(x => x.SendAsync(It.IsAny<ReadOnlyMemory<byte>>(),
                It.IsAny<WebSocketMessageType>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(default(ValueTask));

        // Set Open state as default, special tests for closed states
        SetupGet(x => x.State).Returns(WebSocketState.Open);

        Setup(x => x.ReceiveAsync(It.IsAny<Memory<byte>>(), It.IsAny<CancellationToken>()))
            .Returns(
                async (Memory<byte> buffer, CancellationToken token) =>
                {
                    // The message is read from channel unless we are in a multi-span message
                    var msg = _currentMultiSpanMessage ??
                            await _responseMessageChannel.Reader.ReadAsync(token).ConfigureAwait(false);

                    if (msg.Length - _currentReadPosition > buffer.Length)
                    {
                        // Handle websocket messages that have 
                        // size bigger than buffer, so chunk it up
                        msg.AsMemory(_currentReadPosition, buffer.Length).CopyTo(buffer);
                        _currentMultiSpanMessage = msg;
                        _currentReadPosition += buffer.Length;
                        return new ValueWebSocketReceiveResult(
                            buffer.Length, WebSocketMessageType.Text, false);
                    }

                    var len = msg.Length - _currentReadPosition;
                    msg.AsMemory(_currentReadPosition, len).CopyTo(buffer);

                    _currentReadPosition = 0;
                    _currentMultiSpanMessage = null;
                    return new ValueWebSocketReceiveResult(
                        len, WebSocketMessageType.Text, true);
                });
    }

    public WebSocketState State { get; set; }

    public WebSocketCloseStatus? CloseStatus { get; set; }

    /// <summary>
    ///     Adds a fake response json message that fakes the home assistant server response
    /// </summary>
    /// <param name="message">Message to fake</param>
    public void AddResponse(string message)
    {
        _responseMessageChannel.Writer.TryWrite(Encoding.UTF8.GetBytes(message));
    }
}
