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
}