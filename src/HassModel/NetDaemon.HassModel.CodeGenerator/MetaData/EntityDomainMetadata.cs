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
    private readonly string prefixedDomain = (IsNumeric && EntityIdHelper.MixedDomains.Contains(Domain)  ? "numeric_" : "") + Domain;

    [JsonIgnore]
    public string EntityClassName => GetDomainEntityTypeName(prefixedDomain);

    [JsonIgnore]
    public string AttributesClassName => $"{prefixedDomain}Attributes".ToNormalizedPascalCase();

    [JsonIgnore]
    public string EntitiesForDomainClassName => $"{Domain}Entities".ToNormalizedPascalCase();

    [JsonIgnore]
    public Type? AttributesBaseClass { get; set; }
};

record EntityMetaData(string id, string? friendlyName);

record EntityAttributeMetaData(string JsonName, string CSharpName, Type ClrType);