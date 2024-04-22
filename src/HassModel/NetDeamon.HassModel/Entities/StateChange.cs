namespace NetDaemon.HassModel.Entities;

public interface IStateChange
{
    /// <summary>
    /// The Entity that changed
    /// </summary>
    IEntityCore Entity { get; }

    /// <summary>
    /// The old state of the entity
    /// </summary>
    IEntityState? New { get; }

    /// <summary>
    /// The new state of the entity
    /// </summary>
    IEntityState? Old { get; }
}

/// <summary>
/// Represents a state change event for a strong typed entity and state
/// </summary>
/// <typeparam name="TEntity">The Type</typeparam>
/// <typeparam name="TEntityState"></typeparam>
public record StateChange<TEntity, TEntityState> : IStateChange
    where TEntity : IEntityCore
    where TEntityState : IEntityState
{
    /// <summary>
    /// This should not be used under normal circumstances but can be used for unit testing of apps
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="old"></param>
    /// <param name="new"></param>
    public StateChange(TEntity entity, TEntityState? old, TEntityState? @new)
    {
        Entity = entity;
        New    = @new;
        Old    = old;
    }

    /// <summary>The Entity that changed</summary>
    public virtual TEntity Entity { get; }

    /// <summary>The old state of the entity</summary>
    public virtual TEntityState? New { get; }

    /// <summary>The new state of the entity</summary>
    public virtual TEntityState? Old { get; }
    IEntityCore IStateChange.Entity => Entity;

    IEntityState? IStateChange.New => New;

    IEntityState? IStateChange.Old => Old;
}
