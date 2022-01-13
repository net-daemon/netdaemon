using System.Web;

namespace NetDaemon.Client.Common.HomeAssistant.Extensions;

/// <summary>
///     Adds extensions to the IHomeAssistantApiManager
/// </summary>
public static class IHomeAssistantApiManagerExtensions
{
    /// <summary>
    ///     Sends a custom event to Home Assistant
    /// </summary>
    /// <param name="apiManager">ApiManager to extend</param>
    /// <param name="eventId">Id of the event</param>
    /// <param name="cancellationToken">token to cancel async operation</param>
    /// <param name="data">Data payload to send event</param>
    public static async Task SendEventAsync(this IHomeAssistantApiManager apiManager, string eventId,
        CancellationToken cancellationToken, object? data = null)
    {
        var apiUrl = $"events/{HttpUtility.UrlEncode(eventId)}";

        await apiManager.PostApiCallAsync<object>(apiUrl, cancellationToken, data);
    }
}