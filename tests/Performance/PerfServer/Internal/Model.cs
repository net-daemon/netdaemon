
using System.Collections;

namespace NetDaemon.Tests.Performance;

public record InputBoolean
{
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
    [JsonPropertyName("icon")] public string? Icon { get; init; }
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
}

public record EntityState
{

    [JsonPropertyName("attributes")] public IDictionary<string, object>? Attributes { get; init; } = null;

    [JsonPropertyName("entity_id")] public string EntityId { get; init; } = "";

    [JsonPropertyName("last_changed")] public DateTime LastChanged { get; init; } = DateTime.MinValue;
    [JsonPropertyName("last_updated")] public DateTime LastUpdated { get; init; } = DateTime.MinValue;
    [JsonPropertyName("state")] public string? State { get; init; } = "";
    [JsonPropertyName("context")] public HassContext? Context { get; init; }
}
