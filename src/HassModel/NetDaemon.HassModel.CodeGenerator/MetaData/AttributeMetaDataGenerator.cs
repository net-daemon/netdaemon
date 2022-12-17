using NetDaemon.Client.HomeAssistant.Model;

namespace NetDaemon.HassModel.CodeGenerator;

internal static class AttributeMetaDataGenerator
{
    public static IEnumerable<EntityAttributeMetaData> GetMetaDataFromEntityStates(IEnumerable<HassState> entityStates)
    {
        // Get all attributes of all entities in this set
        var jsonProperties = entityStates.SelectMany(s => s.AttributesJson?.EnumerateObject() ?? Enumerable.Empty<JsonProperty>());

        // Group the attributes from all entities in this set by JsonPropertyName
        var jsonPropetiesByName = jsonProperties.GroupBy(p => p.Name);

        // find the candidate CSharp name and the best ClrType for each unique json property
        var attributesByJsonName = jsonPropetiesByName
            .Select(group => new EntityAttributeMetaData(
                JsonName: group.Key,
                CSharpName: group.Key.ToNormalizedPascalCase(),
                ClrType: GetBestClrType(group.Select(g => g.Value))));

        // We ignore possible duplicate CSharp names here, they will be handled later 
        // by the MetaDataMerger
        return attributesByJsonName;
    }
    private static Type GetBestClrType(IEnumerable<JsonElement> valueKinds)
    {
        var distinctCrlTypes = valueKinds
            .Where(e => e.ValueKind != JsonValueKind.Null) // null fits in any type so we can ignore it for now
            .GroupBy(e => MapJsonType(e.ValueKind))
            .ToHashSet();

        if (distinctCrlTypes.Count is 0 or > 1)
        {
            // Either all inputs where JsonValueKind.Null, or there are multiple possible
            // input types. In either case, the return must be object
            return typeof(object);
        }

        // For arrays, we want to enumerate the sub-elements of the array. If there's a single
        // element type, then we'll construct an IROList<subtype>, otherwise we'll construct
        // IROList<object>
        var clrTypeGroup = distinctCrlTypes.Single();
        if (clrTypeGroup.Key == typeof(IReadOnlyList<>))
        {
            var listSubType = GetBestClrType(clrTypeGroup.SelectMany(el => el.EnumerateArray()));
            return clrTypeGroup.Key.MakeGenericType(listSubType);
        }
        else
        {
            return clrTypeGroup.Key;
        }
    }
    
    private static Type MapJsonType(JsonValueKind kind) =>
        kind switch
        {
            JsonValueKind.False => typeof(bool),
            JsonValueKind.Undefined => typeof(object),
            JsonValueKind.Object => typeof(object),
            JsonValueKind.Array => typeof(IReadOnlyList<>),
            JsonValueKind.String => typeof(string),
            JsonValueKind.Number => typeof(double),
            JsonValueKind.True => typeof(bool),
            JsonValueKind.Null => typeof(object),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
}
