using Moq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetDaemon.Common;

namespace NetDaemon.Daemon.Fakes
{
    /// <summary>
    ///     HttpClient Mock
    /// </summary>
    public class HttpClientFactoryMock : Mock<IHttpClientFactory>
    {
        private HttpClient? _httpClient;
        private MockHttpMessageHandler? _handler;
        /// <summary>
        ///     Message handler for mock
        /// </summary>
        public MockHttpMessageHandler? MessageHandler => _handler;

        /// <summary>
        ///     Public constructor for HttpClientFactoryMock
        /// </summary>
        public HttpClientFactoryMock()
        {
        }

        /// <summary>
        ///     Sets the next response that being faked
        /// </summary>
        /// <param name="response">response from the client</param>
        /// <param name="statusCode">status code being returned</param>
        public void SetResponse(string response, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _handler = new MockHttpMessageHandler(response, statusCode);
            _httpClient = new HttpClient(_handler);
            Setup(x => x.CreateClient(It.IsAny<string>())).Returns(_httpClient!);
        }
    }

    /// <summary>
    ///     Mock of HttpHandler
    /// </summary>
    public class HttpHandlerMock : Mock<IHttpHandler>
    {
        private HttpClient? _httpClient;
        private MockHttpMessageHandler? _handler;

        /// <summary>
        ///     MessageHandler used
        /// </summary>
        public MockHttpMessageHandler? MessageHandler => _handler;

        /// <summary>
        ///     Public constructor
        /// </summary>
        public HttpHandlerMock()
        {
        }

        /// <summary>
        ///     Set response of HttpHandler
        /// </summary>
        /// <param name="response">response to return</param>
        /// <param name="statusCode">status code to return</param>
        public void SetResponse(string response, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _handler = new MockHttpMessageHandler(response, statusCode);
            _httpClient = new HttpClient(_handler);
            Setup(x => x.CreateHttpClient(It.IsAny<string>())).Returns(_httpClient!);
        }
    }

    /// <summary>
    ///     HttpMessageHandler mock
    /// </summary>
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _response;
        private readonly HttpStatusCode _StatusCode;

        private string? _requestContent;

        /// <summary>
        ///     RequestContent of mock
        /// </summary>
        public string? RequestContent => _requestContent;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="response">Respons from client</param>
        /// <param name="statusCode">Status code from client</param>
        public MockHttpMessageHandler(string response, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _response = response;
            _StatusCode = statusCode;
        }

        /// <summary>
        ///     Sends a request to fake mock handler
        /// </summary>
        /// <param name="request">Request to send</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var responseMessage = new HttpResponseMessage(_StatusCode);

            if (request is not null && request.Content is not null)
                _requestContent = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            responseMessage.Content = new ByteArrayContent(Encoding.ASCII.GetBytes(_response));
            return responseMessage;
        }
    }
}