namespace NetDaemon.HassClient.Tests.HomeAssistantClientTest;

public class HomeAssistantConnectionExtensionsTests
{
    [Fact]
    public async Task CallServiceWithResponseShouldUseCallServiceCommandWithReturnResponse()
    {
        var expectedResult = new HassServiceResult
        {
            Context = new HassContext { Id = "context-id" }
        };
        var connectionMock = new Mock<IHomeAssistantConnection>();
        CallServiceCommand? sentCommand = null;
        using var cancelSource = new CancellationTokenSource();
        var target = new HassTarget { EntityIds = ["calendar.cal"] };

        connectionMock
            .Setup(n => n.SendCommandAndReturnResponseAsync<CallServiceCommand, HassServiceResult>(
                It.IsAny<CallServiceCommand>(),
                cancelSource.Token))
            .Callback<CallServiceCommand, CancellationToken>((command, _) => sentCommand = command)
            .ReturnsAsync(expectedResult);

        var result = await connectionMock.Object.CallServiceWithResponseAsync(
            "calendar",
            "get_events",
            new { duration = 60 },
            target,
            cancelSource.Token).ConfigureAwait(false);

        result.Should().BeEquivalentTo(expectedResult);
        sentCommand.Should().NotBeNull();
        sentCommand.Should().BeEquivalentTo(new
        {
            Domain = "calendar",
            Service = "get_events",
            ServiceData = new { duration = 60 },
            Target = target,
            ReturnResponse = (bool?)true
        });
    }

    [Fact]
    public async Task CallServiceWithResponseWithoutCancellationTokenShouldUseCancellationTokenNone()
    {
        var connectionMock = new Mock<IHomeAssistantConnection>();
        CallServiceCommand? sentCommand = null;
        var expectedResult = new HassServiceResult();

        connectionMock
            .Setup(n => n.SendCommandAndReturnResponseAsync<CallServiceCommand, HassServiceResult>(
                It.IsAny<CallServiceCommand>(),
                CancellationToken.None))
            .Callback<CallServiceCommand, CancellationToken>((command, _) => sentCommand = command)
            .ReturnsAsync(expectedResult);

        var result = await connectionMock.Object.CallServiceWithResponseAsync(
            "todo",
            "get_items").ConfigureAwait(false);

        result.Should().BeSameAs(expectedResult);
        sentCommand.Should().NotBeNull();
        sentCommand!.ReturnResponse.Should().BeTrue();
    }
}
