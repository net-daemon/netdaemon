using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetDaemon.Common
{
    /// <summary>
    ///     Implements all Http features of NetDaemon
    /// </summary>
    [SuppressMessage("", "CA1054")]
    public interface IHttpHandler
    {
        /// <summary>
        ///     Returns a http client to use with http calls
        /// </summary>
        /// <param name="name">Logical name of the client to create</param>
        /// <remarks>
        ///     This method uses the HttpClientFactory in the background for
        ///     more resource friendly usage of http client. You can cache the client
        ///     or dispose the client each usage in a using block.
        ///     Callers are also free to mutate the returned HttpClient
        ///     instance's public properties as desired.
        /// </remarks>
        HttpClient CreateHttpClient(string? name = null);

        /// <summary>
        ///     Gets a json resopose deserialized
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="options">Serialization options to use when serializing</param>
        /// <param name="headers">name and value tuple of request headers, allowed values are string and IEnumerable of string</param>
        /// <typeparam name="T">The type to use when deserializing</typeparam>
        Task<T?> GetJson<T>(string url, JsonSerializerOptions? options = null, params (string, object)[] headers);

        /// <summary>
        ///     Post a object that are serialized to a json request and returns a deserializes json response
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="request">The object to use as request</param>
        /// <param name="options">Serialization options to use when serializing</param>
        /// <param name="headers">name and value tuple of request headers, allowed values are string and IEnumerable of string</param>
        /// <typeparam name="T">The type to use when deserializing</typeparam>
        Task<T?> PostJson<T>(string url, object request, JsonSerializerOptions? options = null,
            params (string, object)[] headers);

        /// <summary>
        ///     Post a object that are serialized to a json request
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="request">The object to use as request</param>
        /// <param name="options">Serialization options to use when serializing</param>
        /// <param name="headers">name and value tuple of request headers, allowed values are string and IEnumerable of string</param>
        Task PostJson(string url, object request, JsonSerializerOptions? options = null,
            params (string, object)[] headers);
    }
}