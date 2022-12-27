namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Extension methods for IEntities
/// </summary>
public static class EntityGenericExtensions
{
    /// <summary>
    /// Get a strongly typed IEntity&lt;TState, TAttributes&gt; from any IEntity
    /// This will work if and only if the underlying JSON is compatible with your types.
    /// </summary>
    /// <param name="entity">The source entity</param>
    /// <param name="mapper">The type mapper from raw values to the target types</param>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TAttributes"></typeparam>
    /// <returns></returns>
    public static IEntity<TState, TAttributes> MappedBy<TState, TAttributes>(this IEntity entity, IEntityStateMapper<TState, TAttributes> mapper)
        where TAttributes : class
        => mapper.Map(entity);

    /// <summary>
    /// Get a new IEntity with new state type mapping
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="newStateParser"></param>
    /// <typeparam name="TStateNew"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TAttributes"></typeparam>
    /// <returns></returns>
    public static IEntity<TStateNew, TAttributes> WithStateAsExt<TStateNew, TState, TAttributes>(this IEntity<TState, TAttributes> entity, Func<string?, TStateNew> newStateParser)
        where TAttributes : class
        => entity.EntityStateMapper.WithStateAs(newStateParser).Map(entity);

    /// <summary>
    /// Get a new IEntity with a new attributes class mapping
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="newAttributesParser"></param>
    /// <typeparam name="TAttributesNew"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TAttributes"></typeparam>
    /// <returns></returns>
    public static IEntity<TState, TAttributesNew> WithAttributesAsExt<TAttributesNew, TState, TAttributes>(this IEntity<TState, TAttributes> entity, Func<JsonElement?, TAttributesNew> newAttributesParser)
        where TAttributesNew : class
        where TAttributes : class
        => entity.EntityStateMapper.WithAttributesAs(newAttributesParser).Map(entity);

    /// <summary>
    /// Get a new IEntity with a new attributes class mapping using the default parser
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TAttributesNew"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TAttributes"></typeparam>
    /// <returns></returns>
    public static IEntity<TState, TAttributesNew> WithAttributesAsExt<TAttributesNew, TState, TAttributes>(this IEntity<TState, TAttributes> entity)
        where TAttributesNew : class
        where TAttributes : class
        => entity.EntityStateMapper.WithAttributesAs<TAttributesNew>().Map(entity);
    
    /// <summary>
    /// Get a Numeric IEntity from a given IEntity
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static IEntity<double?, Dictionary<string, object>> ToNumeric(this IEntity entity)
        => DefaultEntityStateMappers.NumericBase.Map(entity);
    
    /// <summary>
    /// Get a DateTime IEntity from a given IEntity 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static IEntity<DateTime?, Dictionary<string, object>> ToDateTime(this IEntity entity)
        => DefaultEntityStateMappers.DateTimeBase.Map(entity);
    
    /// <summary>
    /// Get a Numeric IEntity from a given IEntity
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TAttributes">Is kept unchanged</typeparam>
    /// <returns></returns>
    public static IEntity<double?, TAttributes> AsNumeric<TAttributes>(this IEntity entity)
        where TAttributes : class
        => DefaultEntityStateMappers.NumericTypedAttributes<TAttributes>().Map(entity);
    
    /// <summary>
    /// Get a DateTime IEntity from a given IEntity 
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TAttributes">Is kept unchanged</typeparam>
    /// <returns></returns>
    public static IEntity<DateTime?, TAttributes> AsDateTime<TAttributes>(this IEntity entity)
        where TAttributes : class
        => DefaultEntityStateMappers.DateTimeTypedAttributes<TAttributes>().Map(entity);
    
    /// <summary>
    /// Get a Numeric IEntity from a given IEntity
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TState">Is ignored</typeparam>
    /// <typeparam name="TAttributes">Is kept unchanged</typeparam>
    /// <returns></returns>
    public static IEntity<double?, TAttributes> AsNumeric<TState, TAttributes>(this IEntity<TState, TAttributes> entity)
        where TAttributes : class
        => DefaultEntityStateMappers.NumericTypedAttributes<TAttributes>().Map(entity);
    
    /// <summary>
    /// Get a DateTime IEntity from a given IEntity 
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TState">Is ignored</typeparam>
    /// <typeparam name="TAttributes">Is kept unchanged</typeparam>
    /// <returns></returns>
    public static IEntity<DateTime?, TAttributes> AsDateTime<TState, TAttributes>(this IEntity<TState, TAttributes> entity)
        where TAttributes : class
        => DefaultEntityStateMappers.DateTimeTypedAttributes<TAttributes>().Map(entity);
}