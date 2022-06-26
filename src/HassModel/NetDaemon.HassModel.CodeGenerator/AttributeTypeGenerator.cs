using System.Reflection;
using System.Text.Json.Serialization;
using NetDaemon.HassModel.Entities.Core;

namespace NetDaemon.HassModel.CodeGenerator;

internal class AttributeTypeGenerator
{
    private readonly EntitySet _entitySet;
    private Type[] _baseTypes;

    public AttributeTypeGenerator(CodeGenerationSettings codeGenerationSettings, EntitySet entitySet)
    {
        _baseTypes = codeGenerationSettings.UseAttributeBaseClasses ? typeof(LightAttributesBase).Assembly.GetTypes() : Type.EmptyTypes;
        _entitySet = entitySet;
    }

    /// <summary>
    /// Generates a record with all the attributes found in a set of entities providing unique names for each.
    /// </summary>
    public RecordDeclarationSyntax GenerateAttributeRecord()
    {
        // Get all attributes of all entities in this set
        var jsonProperties = _entitySet.EntityStates.SelectMany(s => s.AttributesJson?.EnumerateObject() ?? Enumerable.Empty<JsonProperty>());

        var baseType = FindBaseClass(_entitySet.AttributesClassName);

        var basePropertyNames = baseType?.GetProperties().Select(p => p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name).ToHashSet() ?? new HashSet<string>();

        // Group the attributes by JsonPropertyName and find the best ClrType that fits all
        var attributesByJsonName = jsonProperties
            .Where(p => !basePropertyNames.Contains(p.Name)) // skip properties already defined in the base class
            .GroupBy(p => p.Name)
            .Select(group => (CSharpName: group.Key.ToNormalizedPascalCase(),
                JsonName: group.Key,
                ClrType: GetBestClrType(group.Select(g => g.Value))));

        // We might have different json names that after CamelCasing result in the same CSharpName 
        var uniqueProperties = attributesByJsonName
            .GroupBy(t => t.CSharpName)
            .SelectMany(DuplicateCSharpName)
            .OrderBy(p => p.CSharpName);

        var propertyDeclarations = uniqueProperties.Select(a => Property($"{a.ClrType.GetFriendlyName()}?", a.CSharpName)
            .ToPublic()
            .WithJsonPropertyName(a.JsonName));

        var record = Record(_entitySet.AttributesClassName, propertyDeclarations).ToPublic();
        return baseType != null ? record.WithBase(SimplifyTypeName(baseType)) : record;
    }

    private Type? FindBaseClass(string typeName)
    {
        return _baseTypes.FirstOrDefault(t => t.Name == typeName + "Base");
    }

    private static IEnumerable<(string CSharpName, string JsonName, Type ClrType)> DuplicateCSharpName(IEnumerable<(string CSharpName, string JsonName, Type ClrType)> items)
    {
        var list = items.ToList();
        if (list.Count == 1) return new[] { list.First() };

        return list.OrderBy(i => i.JsonName).Select((p, i) => ($"{p.CSharpName}_{i}", jsonName: p.JsonName, type: p.ClrType));
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

        // For arrays, we want to enumerate the subelements of the array. If there's a single
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
