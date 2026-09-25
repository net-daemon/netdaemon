namespace NetDaemon.HassClient.Tests.Net;

public class WebSocketTransportPipelineTests
{
    public WebSocketTransportPipelineTests()
    {
        WsMock = new WebSocketClientMock();
        DefaultPipeline = new WebSocketClientTransportPipeline(WsMock.Object);
    }
    private WebSocketClientMock WsMock { get; }
    private WebSocketClientTransportPipeline DefaultPipeline { get; }

    [Fact]
    public async Task TestGetNextMessageAsyncGetsCorrectMessage()
    {
        // ARRANGE
        WsMock.AddResponse(@"{""type"": ""auth_required""}");

        // ACT
        var msg = await DefaultPipeline.GetNextMessagesAsync<HassMessage>(CancellationToken.None).ConfigureAwait(false);

        // ASSERT
        msg[0].Type
            .Should()
            .BeEquivalentTo("auth_required");
    }

    [Fact]
    public async Task TestGetNextMessageAsyncGetsCoalescedMessages()
    {
        // ARRANGE
        WsMock.AddResponse(@"[{""type"": ""event"", ""id"": 1}, {""type"": ""result"", ""id"": 2, ""success"": true}]");

        // ACT
        var msg = await DefaultPipeline.GetNextMessagesAsync<HassMessage>(CancellationToken.None).ConfigureAwait(false);

        // ASSERT
        msg.Should().HaveCount(2);
        msg.Select(n => n.Type).Should().Equal("event", "result");
        msg.Select(n => n.Id).Should().Equal(1, 2);
    }

    [Fact]
    public async Task TestGetNextMessageAsyncGetsCoalescedMessagesWithLeadingWhitespace()
    {
        // ARRANGE
        WsMock.AddResponse(" \r\n\t" + @"[{""type"": ""event"", ""id"": 1}]");

        // ACT
        var msg = await DefaultPipeline.GetNextMessagesAsync<HassMessage>(CancellationToken.None).ConfigureAwait(false);

        // ASSERT
        msg.Should().ContainSingle();
        msg[0].Type.Should().Be("event");
        msg[0].Id.Should().Be(1);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \r\n\t")]
    public async Task TestGetNextMessageAsyncOnEmptyPayloadShouldThrowException(string payload)
    {
        // ARRANGE
        WsMock.AddResponse(payload);

        // ACT AND ASSERT
        await Assert.ThrowsAsync<ApplicationException>(async () =>
            await DefaultPipeline.GetNextMessagesAsync<HassMessage>(CancellationToken.None).ConfigureAwait(false));
    }

    [Fact]
    public async Task TestGetNextMessageAsyncOnJsonNullShouldThrowException()
    {
        // ARRANGE
        WsMock.AddResponse("null");

        // ACT AND ASSERT
        await Assert.ThrowsAsync<ApplicationException>(async () =>
            await DefaultPipeline.GetNextMessagesAsync<HassMessage>(CancellationToken.None).ConfigureAwait(false));
    }

    [Fact]
    public async Task TestGetNextMessageAsyncOnInvalidJsonShouldThrowException()
    {
        // ARRANGE
        WsMock.AddResponse("{not-valid-json");

        // ACT AND ASSERT
        await Assert.ThrowsAsync<JsonException>(async () =>
            await DefaultPipeline.GetNextMessagesAsync<HassMessage>(CancellationToken.None).ConfigureAwait(false));
    }

    [Fact]
    public async Task TestGetNextMessageAsyncOnClosedSocketShouldCastException()
    {
        // ARRANGE
        WsMock.SetupGet(n => n.State).Returns(WebSocketState.Closed);

        // ACT AND ASSERT
        await Assert.ThrowsAsync<ApplicationException>(async () =>
            await DefaultPipeline.GetNextMessagesAsync<HassMessage>(CancellationToken.None).ConfigureAwait(false));
    }

    [Fact]
    public async Task TestGetNextMessageAsyncBigMessageInChunks()
    {
        // ARRANGE
        var sb = new StringBuilder(8192);
        sb.Append(@"{ ""BigChunkedMessage"": """);
        sb.Append(new string('A', 8180));
        sb.Append(@"""}");

        WsMock.AddResponse(sb.ToString());

        // ACT
        var msg = await DefaultPipeline.GetNextMessagesAsync<ChunkedMessagesTestClass>(CancellationToken.None)
            .ConfigureAwait(false);

        msg[0].BigChunkedMessage
            .Should()
            .HaveLength(8180);

        WsMock.Verify(n =>
                n.ReceiveAsync(
                    It.IsAny<Memory<byte>>(),
                    It.IsAny<CancellationToken>()),
            Times.AtLeast(2));
    }

    [Fact]
    public async Task TestSendMessageAsync()
    {
        // ACT
        await DefaultPipeline.SendMessageAsync<object>(new {someMessage = "hello"}, CancellationToken.None)
            .ConfigureAwait(false);

        // ASSERT
        WsMock.Verify(n => n.SendAsync(
            new ArraySegment<byte>(Encoding.UTF8.GetBytes("{\"someMessage\":\"hello\"}")),
            It.IsAny<WebSocketMessageType>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task TestSendMessageAsyncOnClosedSocketShouldTrowException()
    {
        // ARRANGE
        WsMock.SetupGet(n => n.State).Returns(WebSocketState.Closed);

        // ACT AND ASSERT
        await Assert.ThrowsAsync<ApplicationException>(async () =>
            await DefaultPipeline.SendMessageAsync<object>(new {test = "test"}, CancellationToken.None)
                .ConfigureAwait(false));
    }

    [Fact]
    public async Task TestGetNextMessageAsyncOnRemoteClosingWebsocketShouldThrowException()
    {
        // ARRANGE
        WsMock.Setup(n => n.ReceiveAsync(It.IsAny<Memory<byte>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Memory<byte> _, CancellationToken _) =>
            {
                // Simulate a close from remote
                WsMock.SetupGet(n => n.State).Returns(WebSocketState.CloseReceived);
                return new ValueWebSocketReceiveResult(0, WebSocketMessageType.Close, true);
            });


        // ACT AND ASSERT

        // The operation should be cancelled when remote closes websocket
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await DefaultPipeline.GetNextMessagesAsync<HassMessage>(CancellationToken.None).ConfigureAwait(false));

        // CloseOutput should always be called when
        // a close frame are sent from the remote websocket
        WsMock.Verify(n =>
            n.CloseOutputAsync(It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TestGetNextMessageAsyncOnCloseMessageWithoutCloseReceivedShouldRespectCancellation()
    {
        // ARRANGE
        using var cancelSource = new CancellationTokenSource();
        WsMock.Setup(n => n.ReceiveAsync(It.IsAny<Memory<byte>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                cancelSource.Cancel();
                return new ValueWebSocketReceiveResult(0, WebSocketMessageType.Close, true);
            });

        // ACT AND ASSERT
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await DefaultPipeline.GetNextMessagesAsync<HassMessage>(cancelSource.Token).ConfigureAwait(false));

        WsMock.Verify(n =>
                n.CloseOutputAsync(It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TestSendMessageAsyncWithCancelledTokenShouldThrowException()
    {
        // ARRANGE
        using var cancelSource = new CancellationTokenSource();
        await cancelSource.CancelAsync();

        // ACT AND ASSERT
        await Assert.ThrowsAsync<ApplicationException>(async () =>
            await DefaultPipeline.SendMessageAsync<object>(new {test = "test"}, cancelSource.Token)
                .ConfigureAwait(false));
    }

    [Fact]
    public async Task TestDisposeAsyncCallsCloseWhenOpen()
    {
        //ACT
        await DefaultPipeline.DisposeAsync().ConfigureAwait(false);
        WsMock.Verify(n => n.DisposeAsync());
        WsMock.Verify(n =>
            n.CloseAsync(It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TestDisposeAsyncShouldNotThrowExceptionsOnError()
    {
        // ARRANGE
        WsMock.Setup(n =>
                n.CloseAsync(It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Throws(new ApplicationException("what ever"));

        //ACT AND ASSERT
        try
        {
            await DefaultPipeline.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            Assert.Fail("DisposeAsync should not throw exception");
        }
    }

    [Fact]
    public async Task TestCloseAsyncCallsCloseWhenOpen()
    {
        //ACT
        await DefaultPipeline.CloseAsync().ConfigureAwait(false);
        WsMock.Verify(n =>
            n.CloseAsync(It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
    }
}

public record ChunkedMessagesTestClass
{
    public string BigChunkedMessage { get; init; } = string.Empty;
}
