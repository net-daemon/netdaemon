using System.Text.Json;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Extensions;
using NetDaemon.Client.HomeAssistant.Model;

namespace NetDaemon.Tests.Integration.Helpers;

internal static class CalendarEventWaiter
{
    internal const string QueryStartDateTime = "2023-07-26 00:00:00";
    internal const string QueryEndDateTime = "2023-07-28 00:00:00";

    public static Task WaitForEventAsync(
        IHomeAssistantConnection haConnection,
        string summary,
        string description,
        TimeSpan timeout,
        TimeSpan pollInterval)
    {
        return WaitForConditionHelper.WaitUntilAsync(
            async cancellationToken =>
            {
                var result = await haConnection.CallServiceWithResponseAsync(
                    "calendar",
                    "get_events",
                    new
                    {
                        start_date_time = QueryStartDateTime,
                        end_date_time = QueryEndDateTime
                    },
                    new HassTarget { EntityIds = ["calendar.cal"] },
                    cancellationToken).ConfigureAwait(false);

                if (result?.Response is JsonElement response &&
                    response.TryGetProperty("calendar.cal", out var calendarElement))
                {
                    var events = calendarElement.Deserialize<CalendarEvents>();
                    return events?.Events.Any(e => e.Summary == summary && e.Description == description) == true;
                }

                return false;
            },
            timeout,
            pollInterval,
            $"Calendar event '{summary}' was not observed in Home Assistant within the timeout.");
    }
}
