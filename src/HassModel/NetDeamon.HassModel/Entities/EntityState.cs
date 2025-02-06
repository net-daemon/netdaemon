namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Detailed state information
/// </summary>
public record EntityState
{
    /// <summary>Unique id of the entity</summary>
    [JsonPropertyName("entity_id")] public string EntityId { get; init; } = "";

    /// <summary>The state </summary>
    [JsonPropertyName("state")] public string? State { get; init; }

    /// <summary>The attributes as a JsonElement</summary>
    [JsonPropertyName("attributes")] public JsonElement? AttributesJson { get; init; }

    /// <summary>
    /// The attributes
    /// </summary>
    public Dictionary<string, object>? Attributes => AttributesJson?.Deserialize<Dictionary<string, object>>() ?? [];

    /// <summary>Last changed, when state changed from and to different values</summary>
    [JsonPropertyName("last_changed")] public DateTime? LastChanged { get; init; }

    /// <summary>Last updated, when entity state or attributes changed </summary>
    [JsonPropertyName("last_updated")] public DateTime? LastUpdated { get; init; }

    /// <summary>Context</summary>
    [JsonPropertyName("context")] public Context? Context { get; init; }

    internal static TEntityState? Map<TEntityState>(EntityState? state)
        where TEntityState : class =>
        state == null ? null : (TEntityState)Activator.CreateInstance(typeof(TEntityState), state)!;
}

/// <summary>
/// Generic EntityState with specific types of State and Attributes
/// </summary>
/// <typeparam name="TAttributes">The type of the Attributes Property</typeparam>
public record EntityState<TAttributes> : EntityState
    where TAttributes : class
{
    private readonly Lazy<TAttributes?> _attributesLazy;

    /// <summary>
    /// Copy constructor from base class
    /// </summary>
    /// <param name="source"></param>
    public EntityState(EntityState source) : base(source)
    {
        _attributesLazy = new (() => AttributesJson?.Deserialize<TAttributes>() ?? default);
    }

    /// <inheritdoc/>
    public new virtual TAttributes? Attributes => _attributesLazy.Value;
}
