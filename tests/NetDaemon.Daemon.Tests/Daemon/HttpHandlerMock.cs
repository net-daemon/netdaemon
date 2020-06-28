using Moq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetDaemon.Common;

namespace NetDaemon.Daemon.Tests.Daemon
{
    public class HttpClientFactoryMock : Mock<IHttpClientFactory>
    {
        private HttpClient? _httpClient;
        private MockHttpMessageHandler? _handler;
        public MockHttpMessageHandler? MessageHandler => _handler;

        public HttpClientFactoryMock()
        {
        }

        public void SetResponse(string response, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _handler = new MockHttpMessageHandler(response, statusCode);
            _httpClient = new HttpClient(_handler);
            Setup(x => x.CreateClient(It.IsAny<string>())).Returns(_httpClient!);
        }
    }

    public class HttpHandlerMock : Mock<IHttpHandler>
    {
        private HttpClient? _httpClient;
        private MockHttpMessageHandler? _handler;

        public MockHttpMessageHandler? MessageHandler => _handler;

        public HttpHandlerMock()
        {
        }

        public void SetResponse(string response, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _handler = new MockHttpMessageHandler(response, statusCode);
            _httpClient = new HttpClient(_handler);
            Setup(x => x.CreateHttpClient(It.IsAny<string>())).Returns(_httpClient!);
        }
    }

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _response;
        private readonly HttpStatusCode _StatusCode;

        private string? _requestContent;

        public string? RequestContent => _requestContent;

        public MockHttpMessageHandler(string response, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _response = response;
            _StatusCode = statusCode;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var responseMessage = new HttpResponseMessage(_StatusCode);

            if (request is object && request.Content is object)
                _requestContent = await request.Content.ReadAsStringAsync().ConfigureAwait(false);

            responseMessage.Content = new ByteArrayContent(Encoding.ASCII.GetBytes(_response));
            return responseMessage;
        }
    }
}