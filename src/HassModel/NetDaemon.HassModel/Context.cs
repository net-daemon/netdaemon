namespace NetDaemon.HassModel;

/// <summary>
///     Context
/// </summary>
public class Context
{
    /// <summary>
    ///     Id
    /// </summary>
    [JsonPropertyName("id")] public string Id { get; set; } = "";

    /// <summary>
    ///     ParentId
    /// </summary>
    [JsonPropertyName("parent_id")] public string? ParentId { get; set; }
    /// <summary>
    ///     The id of the user who is responsible for the connected item.
    /// </summary>
    [JsonPropertyName("user_id")] public string? UserId { get; set; }
}
