using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NetDaemon.Daemon.Fakes;
using Xunit;

namespace NetDaemon.Daemon.Tests.Daemon
{
    public class SerializedReturn
    {
        [JsonPropertyName("json_prop")] public string? Property { get; set; }
    }

    public class HttpTests : DaemonHostTestBase
    {
        [Fact]
        public async Task HttpClientShouldReturnCorrectContent()
        {
            // ARRANGE
            const string? response = "{\"json_prop\", \"hello world\"}";
            DefaultHttpHandlerMock.SetResponse(response);

            // ACT
            var client = DefaultDaemonHost.Http.CreateHttpClient("test");
            var httpResponseString = await client.GetStringAsync("http://fake.com").ConfigureAwait(false);
            // ASSERT

            Assert.Equal(response, httpResponseString);
        }

        [Fact]
        public void HttpClientShouldNotReturnContentOnBadStatusCode()
        {
            // ARRANGE
            const string? response = "";
            DefaultHttpHandlerMock.SetResponse(response, HttpStatusCode.NotFound);

            // ACT
            var client = DefaultDaemonHost.Http.CreateHttpClient("test");

            // ASSERT
            var ex = Assert.ThrowsAsync<HttpRequestException>(() => client.GetStringAsync("http://fake.com"));
        }

        [Fact]
        public async Task HttpClientShouldReturnCorrectStatusCode()
        {
            // ARRANGE
            const string? response = "{\"json_prop\", \"hello world\"}";
            DefaultHttpHandlerMock.SetResponse(response);

            // ACT
            var client = DefaultDaemonHost.Http.CreateHttpClient("test");
            var httpResponse = await client.GetAsync("http://fake.com").ConfigureAwait(false);
            // ASSERT

            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        }

        [Fact]
        public async Task HttpClientShouldReturnCorrectStatusCodeError()
        {
            // ARRANGE
            const string? response = "{\"json_prop\", \"hello world\"}";
            DefaultHttpHandlerMock.SetResponse(response, HttpStatusCode.Forbidden);

            // ACT
            var client = DefaultDaemonHost.Http.CreateHttpClient("test");
            var httpResponse = await client.GetAsync("http://fake.com").ConfigureAwait(false);
            // ASSERT

            Assert.Equal(HttpStatusCode.Forbidden, httpResponse.StatusCode);
        }

        [Fact]
        public async Task HttpHandlerGetJsonShouldReturnCorrectContent()
        {
            // ARRANGE
            const string? response = "{\"json_prop\": \"hello world\"}";

            using HttpClientFactoryMock factoryMock = new();
            factoryMock.SetResponse(response);

            var httpHandler = new HttpHandler(factoryMock.Object);

            // ACT
            var result = await httpHandler.GetJson<SerializedReturn>("http://fake.com").ConfigureAwait(false);
            // ASSERT

            Assert.Equal("hello world", result?.Property);
        }

        [Fact]
        public void HttpHandlerGetJsonBadFormatShouldReturnThrowException()
        {
            // ARRANGE
            const string? response = "{\"json_prop\": \"hello world\"}";

            using HttpClientFactoryMock factoryMock = new();
            factoryMock.SetResponse(response);

            var httpHandler = new HttpHandler(factoryMock.Object);

            // ACT & ASSERT
            var result = Assert.ThrowsAsync<JsonException>(() => httpHandler.GetJson<SerializedReturn>("http://fake.com"));
        }

        [Fact]
        public async Task HttpHandlerPostJsonShouldReturnCorrectContent()
        {
            // ARRANGE
            const string? response = "{\"json_prop\": \"hello world\"}";

            using HttpClientFactoryMock factoryMock = new();
            factoryMock.SetResponse(response);

            var httpHandler = new HttpHandler(factoryMock.Object);

            // ACT
            var result = await httpHandler.PostJson<SerializedReturn>("http://fake.com", new { posted = "some value" }).ConfigureAwait(false);
            // ASSERT

            Assert.Equal("hello world", result?.Property);
            Assert.Equal("{\"posted\":\"some value\"}", factoryMock.MessageHandler?.RequestContent);
        }

        [Fact]
        public async Task HttpHandlerPostJsonNoReturnShouldReturnCorrectContent()
        {
            // ARRANGE
            const string? response = "{\"json_prop\": \"hello world\"}";

            using HttpClientFactoryMock factoryMock = new();
            factoryMock.SetResponse(response);

            var httpHandler = new HttpHandler(factoryMock.Object);

            // ACT
            await httpHandler.PostJson("http://fake.com", new { posted = "some value" }).ConfigureAwait(false);
            // ASSERT

            Assert.Equal("{\"posted\":\"some value\"}", factoryMock.MessageHandler?.RequestContent);
        }
    }
}