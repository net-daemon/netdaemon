namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Detailed state information
/// </summary>
public record EntityState : EntityState<string, object>
{
    internal static TEntityState? Map<TEntityState>(IEntityState? state)
        where TEntityState : class =>
        state == null ? null : (TEntityState)Activator.CreateInstance(typeof(TEntityState), state)!;    }

public interface IEntityState
{
    string State { get; }
}

/// <summary>
/// Generic EntityState with specific types of State and Attributes
/// </summary>
/// <typeparam name="TAttributes">The type of the Attributes Property</typeparam>
/// <typeparam name="TState"></typeparam>
public record EntityState<TState, TAttributes> : IEntityState
    where TAttributes : class
{
    private readonly Lazy<TAttributes?> _attributesLazy;

    public string EntityId { get; init; } = "";

    /// <summary>The attributes as a JsonElement</summary>
    public JsonElement? AttributesJson { get; init; }

    /// <summary>The state </summary>
    public TState? State { get; init; }

    string IEntityState.State => _state;
    private string _state { get; }

    /// <summary>Last changed, when state changed from and to different values</summary>
    public DateTime? LastChanged { get; init; }

    /// <summary>Last updated, when entity state or attributes changed </summary>
    public DateTime? LastUpdated { get; init; }

    /// <summary>Context</summary>
    public Context? Context { get; init; }

    /// <inheritdoc/>
    public virtual TAttributes? Attributes => _attributesLazy.Value;

    /// <summary>
    /// Copy constructor from base class
    /// </summary>
    /// <param name="source"></param>
    public EntityState()
    {
        _attributesLazy = new (() => AttributesJson?.Deserialize<TAttributes>() ?? default);
    }

    /// <summary>
    /// Copy constructor from base class
    /// </summary>
    /// <param name="source"></param>
    public EntityState(IEntityState state)
    {
        _attributesLazy = new (() => AttributesJson?.Deserialize<TAttributes>() ?? default);
        _state = state.State;
    }
}
