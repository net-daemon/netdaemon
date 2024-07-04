using System.Text.Json;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.Client;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace Apps;

[NetDaemonApp]
public sealed class ServiceApp : IAsyncInitializable
{
    private readonly IHaContext _ha;
    private readonly IHomeAssistantApiManager _api;
    private readonly HttpClient _client;
    private readonly ILogger<ServiceApp> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ServiceApp(IHaContext ha, IHomeAssistantApiManager api, HttpClient client,  ILogger<ServiceApp> logger)
    {
        _ha = ha;
        _api = api;
        _client = client;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var result = await _ha.CallServiceWithResponseAsync("calendar", "get_events", ServiceTarget.FromEntity("calendar.cal"),
            data: new { start_date_time = "2023-07-21 00:00:00", end_date_time = "2023-07-22 03:00:00"});

        if (result is not null)
        {
            var events = result.Value.GetProperty("calendar.cal").Deserialize<CalendarEvents>(_jsonOptions);
            if (events is null)
                _logger.LogWarning("No results!");
            else
                _logger.LogInformation("Events: {Events}", events);
        }
    }
}
public record CalendarEvents(IEnumerable<CalendarEvent> Events);
public record CalendarEvent(DateTime Start, DateTime End, string Summary, string Description);
