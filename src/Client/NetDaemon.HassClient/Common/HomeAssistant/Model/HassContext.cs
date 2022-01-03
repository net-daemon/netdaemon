namespace NetDaemon.Client.Common.HomeAssistant.Model;

public record HassContext
{
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;

    [JsonPropertyName("parent_id")] public string? ParentId { get; init; }

    [JsonPropertyName("user_id")] public string? UserId { get; init; }
}