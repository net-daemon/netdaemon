using System.Web;

namespace NetDaemon.Client.Common.HomeAssistant.Extensions;

public static class IHomeAssistantApiManagerExtensions
{
    public static async Task SendEventAsync(this IHomeAssistantApiManager apiManager, string eventId,
        CancellationToken cancellationToken, object? data = null)
    {
        var apiUrl = $"events/{HttpUtility.UrlEncode(eventId)}";

        await apiManager.PostApiCallAsync<object>(apiUrl, cancellationToken, data);
    }
}