namespace NetDaemon.HassClient.Tests.HelperTest;

public class HttpHandlerHelperTests
{
    [Fact]
    public void TestHttpHandlerHelperCreateClient()
    {
        var client = HttpHelper.CreateHttpClient();
        client.Should().BeOfType<HttpClient>();
    }

    [Fact]
    public void TestHttpHandlerHelperCreateHttpMessageHandler()
    {
        var client = HttpHelper.CreateHttpMessageHandler();
        client.Should().BeOfType<HttpClientHandler>();
    }

    [Fact]
    public void TestHttpHandlerHelperCreateHttpMessageHandlerIgnoreCertErrors()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Options.Create(new HomeAssistantSettings
            {InsecureBypassCertificateErrors = true}));
        
        var provider = services.BuildServiceProvider();

        // Act
        var client = HttpHelper.CreateHttpMessageHandler(provider);

        // Assert
        client.Should().BeOfType<HttpClientHandler>();
    }
}