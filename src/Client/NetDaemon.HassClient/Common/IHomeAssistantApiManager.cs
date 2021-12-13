
namespace NetDaemon.Client.Common;

public interface IHomeAssistantApiManager
{
    /// <summary>
    ///     Get to Home Assistant API
    /// </summary>
    /// <param name="apiPath">relative path</param>
    /// <param name="cancelToken">cancellation token</param>
    /// <typeparam name="T">Return type (json serializable)</typeparam>
    Task<T?> GetApiCallAsync<T>(string apiPath, CancellationToken cancelToken);

    /// <summary>
    ///     Post to Home Assistant API
    /// </summary>
    /// <param name="apiPath">relative path</param>
    /// <param name="cancelToken">cancellation token</param>
    /// <param name="data">data being sent</param>
    /// <typeparam name="T">Return type (json serializable)</typeparam>
    public Task<T?> PostApiCallAsync<T>(string apiPath, CancellationToken cancelToken, object? data = null);
}
