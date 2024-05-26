namespace NetDaemon.Client.Internal.Helpers;

internal static class HttpHelper
{
    [SuppressMessage("", "CA2000")]
    public static HttpClient CreateHttpClient()
    {
        return new HttpClient(CreateHttpMessageHandler());
    }

    public static HttpMessageHandler CreateHttpMessageHandler(IServiceProvider? serviceProvider = null)
    {
        var settings = serviceProvider?.GetService<IOptions<HomeAssistantSettings>>()?.Value;
        var bypassCertificateErrors = settings?.InsecureBypassCertificateErrors ?? false;
        return !bypassCertificateErrors
            ? new HttpClientHandler()
            : CreateHttpMessageHandler();
    }

    private static HttpMessageHandler CreateHttpMessageHandler()
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, sslPolicyErrors) => sslPolicyErrors == SslPolicyErrors.None || true
        };
    }
}
