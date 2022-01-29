namespace NetDaemon.HassClient.Tests.ExtensionsTest;

public class HomeAssistantApiManagerExtensionTests
{
    [Fact]
    public async Task SendEventAsyncShouldCallCorrectFunctions()
    {
        var apiManagerMock = new Mock<IHomeAssistantApiManager>();
        var data = new {anydata = "hello"};
        await apiManagerMock.Object.SendEventAsync("eventId", CancellationToken.None, data).ConfigureAwait(false);
        apiManagerMock.Verify(n => n.PostApiCallAsync<object>("events/eventId", It.IsAny<CancellationToken>(), data),
            Times.Once);
    }

    [Fact]
    public async Task GetEntityStateAsyncShouldCallCorrectFunctions()
    {
        var apiManagerMock = new Mock<IHomeAssistantApiManager>();
        await apiManagerMock.Object.GetEntityStateAsync("entityId", CancellationToken.None).ConfigureAwait(false);
        apiManagerMock.Verify(
            n => n.GetApiCallAsync<HassState>("states/entityId", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetEntityStateAsyncShouldCallCorrectFunctions()
    {
        var apiManagerMock = new Mock<IHomeAssistantApiManager>();
        await apiManagerMock.Object
            .SetEntityStateAsync("entityId", "state", new {attr = "attr"}, CancellationToken.None)
            .ConfigureAwait(false);
        apiManagerMock.Verify(
            n => n.PostApiCallAsync<HassState>("states/entityId", It.IsAny<CancellationToken>(), It.IsAny<object?>()),
            Times.Once);

        var argData = apiManagerMock.Invocations.First().Arguments[2]!;
        argData.GetType().GetProperty("state")!.GetValue(argData, null)!.Should()
            .Be("state");
        var attributes = argData.GetType().GetProperty("attributes")!.GetValue(argData, null)!;
        attributes.GetType().GetProperty("attr")!.GetValue(attributes, null).Should().Be("attr");
    }
}
