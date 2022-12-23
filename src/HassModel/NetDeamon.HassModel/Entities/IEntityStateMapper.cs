namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Maps the raw state and attributes of an IEntity object to strongly typed objects / values
/// </summary>
/// <typeparam name="TState">A type that is deserializable from a nullable string to hold the state of the entity</typeparam>
/// <typeparam name="TAttributes">Json deserializable reference type to hold the entities attributes</typeparam>
public interface IEntityStateMapper<TState, TAttributes>
    where TAttributes : class
{
    /// <summary>
    /// Parse a nullable string into the state type
    /// </summary>
    /// <param name="rawState"></param>
    /// <returns></returns>
    TState ParseState(string? rawState);

    /// <summary>
    /// Parse a nullable JsonElement into the strongly type attribute class
    /// </summary>
    /// <param name="rawAttributes"></param>
    /// <returns></returns>
    TAttributes? ParseAttributes(JsonElement? rawAttributes);

    /// <summary>
    /// Map a raw entity state object into a strongly typed one
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    IEntityState<TState, TAttributes> Map(IEntityState state);

    /// <summary>
    /// Map a raw entity into a strongly typed one
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    IEntity<TState, TAttributes> Map(IEntity entity);

    /// <summary>
    /// Map a raw state change object into a strongly typed one
    /// </summary>
    /// <param name="stateChange"></param>
    /// <returns></returns>
    IStateChange<TState, TAttributes> Map(IStateChange stateChange);
}