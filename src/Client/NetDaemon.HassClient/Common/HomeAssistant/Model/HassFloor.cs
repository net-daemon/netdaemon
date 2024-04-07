namespace NetDaemon.Client.HomeAssistant.Model;

public record HassFloor
{
    [JsonPropertyName("level")] public short? Level { get; init; }

    [JsonPropertyName("description")] public string? Description { get; init; }

    [JsonPropertyName("icon")] public string? Icon { get; init; }

    [JsonPropertyName("floor_id")] public string? Id { get; init; }

    [JsonPropertyName("name")] public string? Name { get; init; }
}
/*

*/
