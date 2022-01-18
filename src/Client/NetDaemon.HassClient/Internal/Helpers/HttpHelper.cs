namespace NetDaemon.Client.Internal.Helpers;

internal static class HttpHelper
{
    [SuppressMessage("", "CA2000")]
    public static HttpClient CreateHttpClient()
    {
        return new HttpClient(CreateHttpMessageHandler());
    }

    public static HttpMessageHandler CreateHttpMessageHandler()
    {
        var bypassCertificateErrorsForHash = Environment.GetEnvironmentVariable("HASSCLIENT_BYPASS_CERT_ERR");
        return string.IsNullOrEmpty(bypassCertificateErrorsForHash)
            ? new HttpClientHandler()
            : CreateHttpMessageHandler(bypassCertificateErrorsForHash);
    }

    private static HttpMessageHandler CreateHttpMessageHandler(string certificate)
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, cert, _, sslPolicyErrors) =>
            {
                if (sslPolicyErrors == SslPolicyErrors.None) return true; //Is valid

                return cert?.GetCertHashString() == certificate.ToUpperInvariant();
            }
        };
    }
}