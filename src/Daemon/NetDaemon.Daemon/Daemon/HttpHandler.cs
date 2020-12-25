using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using NetDaemon.Common;

namespace NetDaemon.Daemon
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

        public async Task<T?> GetJson<T>(string url, JsonSerializerOptions? options = null, params (string, object)[] headers)
        {
            _ = _httpClientFactory ?? throw new NullReferenceException("No IHttpClientFactory provided, please add AddHttpClient() in configure services!");

            var httpClient = _httpClientFactory.CreateClient();

            AddHeaders(httpClient, headers);

            var streamTask = httpClient.GetStreamAsync(url)
                ?? throw new ApplicationException($"Unexpected, nothing returned from {url}");

            return await JsonSerializer.DeserializeAsync<T>(await streamTask.ConfigureAwait(false), options).ConfigureAwait(false);
        }

        public async Task<T?> PostJson<T>(string url, object request, JsonSerializerOptions? options = null, params (string, object)[] headers)
        {
            _ = _httpClientFactory ?? throw new NullReferenceException("No IHttpClientFactory provided, please add AddHttpClient() in configure services!");

            var httpClient = _httpClientFactory.CreateClient();

            AddHeaders(httpClient, headers);

            var bytesToPost = JsonSerializer.SerializeToUtf8Bytes(request, request.GetType(), options);

            var response = await httpClient.PostAsync(url, new ByteArrayContent(bytesToPost)).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var streamTask = response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(await streamTask.ConfigureAwait(false)).ConfigureAwait(false);
        }

        public async Task PostJson(string url, object request, JsonSerializerOptions? options = null, params (string, object)[] headers)
        {
            _ = _httpClientFactory ?? throw new NullReferenceException("No IHttpClientFactory provided, please add AddHttpClient() in configure services!");

            var httpClient = _httpClientFactory.CreateClient();

            AddHeaders(httpClient, headers);

            var bytesToPost = JsonSerializer.SerializeToUtf8Bytes(request, request.GetType(), options);

            var response = await httpClient.PostAsync(url, new ByteArrayContent(bytesToPost)).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
        }

        private static void AddHeaders(HttpClient httpClient, (string, object)[] headers)
        {
            if (headers is not null && headers.Length > 0)
            {
                httpClient.DefaultRequestHeaders.Clear();
                foreach (var (name, header) in headers)
                {
                    if (header is string headerStr)
                        httpClient.DefaultRequestHeaders.Add(name, headerStr);
                    else if (header is IEnumerable<string> headerStrings)
                        httpClient.DefaultRequestHeaders.Add(name, headerStrings);
                    else
                        throw new ApplicationException($"Unsupported header, expected string or IEnumerable<string> for {name}");
                }
            }
        }
    }
}