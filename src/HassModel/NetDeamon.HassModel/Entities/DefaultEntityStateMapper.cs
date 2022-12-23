namespace NetDaemon.HassModel.Entities;

/// <summary>
/// A Collection of useful `EntityStateMapper` instances
/// </summary>
public static class DefaultEntityStateMappers
{
    private static string? StateAsString(string? s) => s;

    private static Dictionary<string, object> AttributesAsObjectDictionary(JsonElement? a) =>
        a?.Deserialize<Dictionary<string, object>>() ?? new Dictionary<string, object>();

    /// <summary>
    /// Parses the attributes JSON into the class TAttributes
    /// </summary>
    /// <typeparam name="TAttributes"></typeparam>
    public static TAttributes? AttributesAsClass<TAttributes>(JsonElement? a) where TAttributes : class =>
        a?.Deserialize<TAttributes>() ?? default;

    /// <summary>
    /// Matches the types of the original `Entity` class
    /// </summary>
    /// <returns></returns>
    public static IEntityStateMapper<string?, Dictionary<string, object>> Base =>
        new EntityStateMapper<string?, Dictionary<string, object>>(StateAsString, AttributesAsObjectDictionary);

    /// <summary>
    /// Matches the types of the Original `Entity&lt;TAttributes&gt;` class
    /// </summary>
    /// <typeparam name="TAttributes"></typeparam>
    public static IEntityStateMapper<string?, TAttributes> TypedAttributes<TAttributes>() where TAttributes : class =>
        new EntityStateMapper<string?, TAttributes>(StateAsString, AttributesAsClass<TAttributes>);

    /// <summary>
    /// Matches the types of the original NumericEntity class
    /// </summary>
    /// <returns></returns>
    public static IEntityStateMapper<double?, Dictionary<string, object>> NumericBase =>
        new EntityStateMapper<double?, Dictionary<string, object>>(FormatHelpers.ParseAsDouble, AttributesAsObjectDictionary);

    /// <summary>
    /// Matches the types of the Original NumericEntity&lt;TAttributes&gt; class
    /// </summary>
    /// <typeparam name="TAttributes"></typeparam>
    public static IEntityStateMapper<double?, TAttributes> NumericTypedAttributes<TAttributes>() where TAttributes : class =>
        new EntityStateMapper<double?, TAttributes>(FormatHelpers.ParseAsDouble, AttributesAsClass<TAttributes>);

    /// <summary>
    /// Parse the state as a DateTime
    /// </summary>
    /// <returns></returns>
    public static IEntityStateMapper<DateTime?, Dictionary<string, object>> DateTimeBase =>
        new EntityStateMapper<DateTime?, Dictionary<string, object>>(s => s is null ? null : DateTime.Parse(s), AttributesAsObjectDictionary);

    /// <summary>
    /// Parse the state as a DateTime and parse the attributes into a class
    /// </summary>
    /// <typeparam name="TAttributes"></typeparam>
    public static IEntityStateMapper<DateTime?, TAttributes> DateTimeTypedAttributes<TAttributes>() where TAttributes : class =>
        new EntityStateMapper<DateTime?, TAttributes>(s => s is null ? null : DateTime.Parse(s), AttributesAsClass<TAttributes>);
}