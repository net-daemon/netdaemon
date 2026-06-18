using System.Text;

namespace NetDaemon.PerformanceBenchmarks.Support;

internal static class HassPayloadFactory
{
    public static string StateChangedEvent(int index)
    {
        return $$"""
            {
              "id": 2,
              "type": "event",
              "event": {
                "event_type": "state_changed",
                "data": {
                  "entity_id": "sensor.perf_{{index}}",
                  "old_state": {
                    "entity_id": "sensor.perf_{{index}}",
                    "state": "{{index - 1}}",
                    "attributes": { "unit_of_measurement": "W", "friendly_name": "Perf {{index}}" },
                    "last_changed": "2026-06-18T10:00:00Z",
                    "last_updated": "2026-06-18T10:00:00Z"
                  },
                  "new_state": {
                    "entity_id": "sensor.perf_{{index}}",
                    "state": "{{index}}",
                    "attributes": { "unit_of_measurement": "W", "friendly_name": "Perf {{index}}" },
                    "last_changed": "2026-06-18T10:00:01Z",
                    "last_updated": "2026-06-18T10:00:01Z",
                    "context": { "id": "ctx-{{index}}", "parent_id": null, "user_id": null }
                  }
                },
                "origin": "LOCAL",
                "time_fired": "2026-06-18T10:00:01Z"
              }
            }
            """;
    }

    public static string CoalescedStateChangedEvents(int count)
    {
        var builder = new StringBuilder(count * 700);
        builder.Append('[');
        for (var i = 0; i < count; i++)
        {
            if (i > 0) builder.Append(',');
            builder.Append(StateChangedEvent(i));
        }
        builder.Append(']');
        return builder.ToString();
    }

    public static string ResultMessage(int id)
    {
        return $$"""
            {
              "id": {{id}},
              "type": "result",
              "success": true,
              "result": { "ok": true }
            }
            """;
    }
}
