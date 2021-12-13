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
        Environment.SetEnvironmentVariable("HASSCLIENT_BYPASS_CERT_ERR", "somehash");
        var client = HttpHelper.CreateHttpMessageHandler();
        client.Should().BeOfType<HttpClientHandler>();
    }
}