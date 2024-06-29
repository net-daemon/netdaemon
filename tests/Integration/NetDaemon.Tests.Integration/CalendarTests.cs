using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.AppModel;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetDaemon.Tests.Integration.Helpers;
using Xunit;

namespace NetDaemon.Tests.Integration;

[NetDaemonApp]
public class CalendarApp
{
    public CalendarApp(
        IHaContext haContext
    )
    {
        // We wait for the test to send an event to start the test and we send back a event with the result
        haContext.Events.Where(s => s.EventType == "start_test_custom_calendar_events").SubscribeAsync(async _ =>
        {
            var result = await haContext.CallServiceWithResponseAsync("calendar", "get_events",
                ServiceTarget.FromEntity("calendar.cal"),
                new { start_date_time = "2023-07-26 00:00:00", end_date_time = "2023-07-28 00:00:00" });

            if (result is not null)
            {
                var events = result.Value.GetProperty("calendar.cal").Deserialize<CalendarEvents>();
                if (events is not null)
                    // This is a way to get the results back in the tests since app instances are not available
                    haContext.SendEvent("custom_calendar_events", new { events });
            }
        });
    }

    public CalendarEvents? Events { get; set; }
}

public record CalendarEvents
{
        [JsonPropertyName("events")] public List<CalendarEvent> Events { get; init; } = [];
}

public record CalendarEvent
{
    [JsonPropertyName("start")] public DateTime Start { get; init; }
    [JsonPropertyName("end")] public DateTime End { get; init; }
    [JsonPropertyName("summary")] public string Summary { get; init; } = default!;
    [JsonPropertyName("description")] public string Description { get; init; } = default!;
}

public record CreateCalendarEvent
{
    [JsonPropertyName("dtstart")] public DateTime Start { get; init; }
    [JsonPropertyName("dtend")] public DateTime End { get; init; }
    [JsonPropertyName("summary")] public string Summary { get; init; } = default!;
    [JsonPropertyName("description")] public string Description { get; init; } = default!;
}

public record AddCalendarEventCommand : CommandMessage
{
    [JsonPropertyName("entity_id")] public string EntityId { get; init; } = default!;
    [JsonPropertyName("event")] public CreateCalendarEvent Event { get; init; } = default!;
}

public class CalendarTests : NetDaemonIntegrationBase
{
    public CalendarTests(HomeAssistantLifetime homeAssistantLifetime) : base(homeAssistantLifetime)
    {
    }

    /// <summary>
    ///     Test the calendar app returning events using CallServiceWithResponse
    /// </summary>
    /// <remarks>
    ///     This test tests the CallServiceWithResponse using the Calendar integration.
    ///     The integration is setup part of setting up the HomeAssistantTestContainer
    ///
    ///     The test uses HA events to start test and return result
    ///     since app instances are not available in tests
    ///     1. Create a calendar item to test
    ///     2. Start the test by sending an event
    ///     3. Wait for the result event
    /// </remarks>
    [Fact]
    public async Task CalendarApp_ShouldReturnEvents()
    {
        var haContext = Services.GetRequiredService<IHaContext>();
        var haConnection = Services.GetRequiredService<IHomeAssistantConnection>();

        // First create the calendar item that will be returned
        await AddTestCalendarItem();
        await Task.Delay(500); // Wait for the event to be processed

        // Then we create a task that waits for the result using a custom event
        var waitTask = haContext.Events.Where(e => e.EventType == "custom_calendar_events").FirstAsync().ToTask();
        var act = async () => await waitTask.ConfigureAwait(false);

        // Trigger the start of the test
        haContext.SendEvent("start_test_custom_calendar_events");
        var result = (await act.Should().CompleteWithinAsync(5000.Milliseconds())).Subject;

        // Check the content of events returned
        result!.DataElement!.Should().NotBeNull();
        var events = result.DataElement!.Value.GetProperty("events").Deserialize<CalendarEvents>();
        events!.Events.Should().NotBeNull();
        events!.Events.Count.Should().Be(1);
        events!.Events[0].Summary.Should().Be("Test");

        async Task AddTestCalendarItem()
        {
            await haConnection.SendCommandAndReturnResponseRawAsync(new AddCalendarEventCommand
            {
                Type = "calendar/event/create",
                EntityId = "calendar.cal",
                Event = new CreateCalendarEvent
                {
                    Summary = "Test",
                    Description = "A test calendar event",
                    Start = DateTime.Parse("2023-07-27T22:00:00", CultureInfo.InvariantCulture),
                    End = DateTime.Parse("2023-07-27T23:00:00", CultureInfo.InvariantCulture)
                }
            }, CancellationToken.None);
        }
    }
}
