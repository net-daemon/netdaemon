using System.Text.Json.Serialization;

namespace NetDaemon.HassModel.CodeGenerator;

record EntitiesMetaData
{
    public IReadOnlyCollection<EntityDomainMetadata> Domains { get; init; } = Array.Empty<EntityDomainMetadata>();
}

record EntityDomainMetadata(
    string Domain,

    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    bool IsNumeric,

    IReadOnlyList<EntityMetaData> Entities,

    IReadOnlyList<EntityAttributeMetaData> Attributes
    )
{
    private static readonly HashSet<string> CoreInterfaces =
        typeof(IEntityCore).Assembly.GetTypes()
            .Where(t => t.IsInterface && t.IsAssignableTo(typeof(IEntityCore)))
            .Select(t => t.Name)
            .ToHashSet();

    private readonly string prefixedDomain = (IsNumeric && EntityIdHelper.MixedDomains.Contains(Domain)  ? "numeric_" : "") + Domain;

    [JsonIgnore]
    public string EntityClassName => $"{prefixedDomain}Entity".ToValidCSharpPascalCase();

    /// <summary>
    /// Returns the name of the corresponding Core Interface if it exists, or null if it does not
    /// </summary>
    [JsonIgnore]
    public string? CoreInterfaceName
    {
        get
        {
            var name = $"I{Domain.ToValidCSharpPascalCase()}EntityCore";
            return CoreInterfaces.Contains(name) ? name : null;
        }
    }

    [JsonIgnore]
    public string AttributesClassName => $"{prefixedDomain}Attributes".ToValidCSharpPascalCase();

    [JsonIgnore]
    public string EntitiesForDomainClassName => $"{Domain}Entities".ToValidCSharpPascalCase();

    [JsonIgnore]
    public Type? AttributesBaseClass { get; set; }
};

record EntityMetaData(string id, string? friendlyName, string cSharpName);

record EntityAttributeMetaData(string JsonName, string CSharpName, Type? ClrType);
