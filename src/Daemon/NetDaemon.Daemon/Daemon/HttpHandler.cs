using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common;

namespace JoySoftware.HomeAssistant.NetDaemon.Daemon
{
    public class HttpHandler : IHttpHandler
    {
        private readonly IHttpClientFactory? _httpClientFactory;

        public HttpHandler(IHttpClientFactory? httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public HttpClient CreateHttpClient(string? name = null)
        {
            _ = _httpClientFactory ?? throw new NullReferenceException("No IHttpClientFactory provided, please add AddHttpClient() in configure services!");
            return _httpClientFactory.CreateClient(name);
        }

        public async Task<T> GetJson<T>(string url, JsonSerializerOptions? options = null, params (string, object)[] headers)
        {
            _ = _httpClientFactory ?? throw new NullReferenceException("No IHttpClientFactory provided, please add AddHttpClient() in configure services!");

            var httpClient = _httpClientFactory.CreateClient();

            AddHeaders(httpClient, headers);

            var streamTask = httpClient.GetStreamAsync(url);

            return await JsonSerializer.DeserializeAsync<T>(await streamTask.ConfigureAwait(false), options);
        }

        public async Task<T> PostJson<T>(string url, object request, JsonSerializerOptions? options = null, params (string, object)[] headers)
        {
            _ = _httpClientFactory ?? throw new NullReferenceException("No IHttpClientFactory provided, please add AddHttpClient() in configure services!");

            var httpClient = _httpClientFactory.CreateClient();

            AddHeaders(httpClient, headers);

            var bytesToPost = JsonSerializer.SerializeToUtf8Bytes(request, request.GetType(), options);

            var response = await httpClient.PostAsync(url, new ByteArrayContent(bytesToPost));

            response.EnsureSuccessStatusCode();

            var streamTask = response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(await streamTask.ConfigureAwait(false));
        }

        public async Task PostJson(string url, object request, JsonSerializerOptions? options = null, params (string, object)[] headers)
        {
            _ = _httpClientFactory ?? throw new NullReferenceException("No IHttpClientFactory provided, please add AddHttpClient() in configure services!");

            var httpClient = _httpClientFactory.CreateClient();

            AddHeaders(httpClient, headers);

            var bytesToPost = JsonSerializer.SerializeToUtf8Bytes(request, request.GetType(), options);

            var response = await httpClient.PostAsync(url, new ByteArrayContent(bytesToPost));

            response.EnsureSuccessStatusCode();
        }

        private void AddHeaders(HttpClient httpClient, (string, object)[] headers)
        {
            if (headers is object && headers.Length > 0)
            {
                httpClient.DefaultRequestHeaders.Clear();
                foreach (var (name, header) in headers)
                {
                    if (header is string)
                        httpClient.DefaultRequestHeaders.Add(name, (string)header);
                    else if (header is IEnumerable<string>)
                        httpClient.DefaultRequestHeaders.Add(name, (IEnumerable<string>)header);
                    else
                        throw new ApplicationException($"Unsupported header, expected string or IEnumerable<string> for {name}");
                }
            }
        }
    }
}