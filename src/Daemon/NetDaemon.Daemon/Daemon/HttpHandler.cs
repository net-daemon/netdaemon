using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using NetDaemon.Common;
using NetDaemon.Common.Exceptions;

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
            _ = _httpClientFactory ?? throw new NetDaemonNullReferenceException("No IHttpClientFactory provided, please add AddHttpClient() in configure services!");
            return _httpClientFactory.CreateClient(name);
        }

        public async Task<T?> GetJson<T>(string url, JsonSerializerOptions? options = null, params (string, object)[] headers)
        {
            _ = _httpClientFactory ?? throw new NetDaemonNullReferenceException("No IHttpClientFactory provided, please add AddHttpClient() in configure services!");

            using var httpClient = _httpClientFactory.CreateClient();

            AddHeaders(httpClient, headers);

            var streamTask = httpClient.GetStreamAsync(new Uri(url))
                ?? throw new NetDaemonException($"Unexpected, nothing returned from {url}");

            return await JsonSerializer.DeserializeAsync<T>(await streamTask.ConfigureAwait(false), options).ConfigureAwait(false);
        }

        public async Task<T?> PostJson<T>(string url, object request, JsonSerializerOptions? options = null, params (string, object)[] headers)
        {
            _ = _httpClientFactory ?? throw new NetDaemonNullReferenceException("No IHttpClientFactory provided, please add AddHttpClient() in configure services!");
            _ = request ?? throw new NetDaemonArgumentNullException(nameof(request));

            using var httpClient = _httpClientFactory.CreateClient();

            AddHeaders(httpClient, headers);

            var bytesToPost = JsonSerializer.SerializeToUtf8Bytes(request, request.GetType(), options);
            using var content = new ByteArrayContent(bytesToPost);

            var response = await httpClient.PostAsync(new Uri(url), content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var streamTask = response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(await streamTask.ConfigureAwait(false)).ConfigureAwait(false);
        }

        public async Task PostJson(string url, object request, JsonSerializerOptions? options = null, params (string, object)[] headers)
        {
            _ = _httpClientFactory ?? throw new NetDaemonNullReferenceException("No IHttpClientFactory provided, please add AddHttpClient() in configure services!");
            _ = request ?? throw new NetDaemonArgumentNullException(nameof(request));

            using var httpClient = _httpClientFactory.CreateClient();

            AddHeaders(httpClient, headers);

            var bytesToPost = JsonSerializer.SerializeToUtf8Bytes(request, request.GetType(), options);
            using var content = new ByteArrayContent(bytesToPost);

            var response = await httpClient.PostAsync(new Uri(url), content).ConfigureAwait(false);

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
                        throw new NetDaemonException($"Unsupported header, expected string or IEnumerable<string> for {name}");
                }
            }
        }
    }
}