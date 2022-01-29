namespace NetDaemon.HassClient.Tests.Net;

public class WebSocketClientTests
{
    [Fact]
    public void TestFactoryReturnCorrectType()
    {
        WebSocketClientFactory wsFactory = new(Options.Create<HomeAssistantSettings>(new()));
        Assert.True(wsFactory.New() is WebSocketClientImpl);
    }
}
