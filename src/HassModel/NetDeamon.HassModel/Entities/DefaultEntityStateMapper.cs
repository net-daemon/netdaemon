namespace NetDaemon.HassModel.Entities;

public static class DefaultEntityStateMappers
{
    private static string? StateAsString(string? s) => s;

    private static Dictionary<string, object> AttributesAsObjectDictionary(JsonElement? a) =>
        a?.Deserialize<Dictionary<string, object>>() ?? new Dictionary<string, object>();

    public static IEntityStateMapper<string?, Dictionary<string, object>> Base =>
        new EntityStateMapper<string?, Dictionary<string, object>>(StateAsString, AttributesAsObjectDictionary);

    public static IEntityStateMapper<double?, Dictionary<string, object>> NumericBase =>
        new EntityStateMapper<double?, Dictionary<string, object>>(FormatHelpers.ParseAsDouble, AttributesAsObjectDictionary);
}