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
    public string EntityClassName => $"{prefixedDomain}Entity".ToValidCSharpPascalCase();

    [JsonIgnore]
    public string CoreInterfaceName => $"I{Domain.ToValidCSharpPascalCase()}EntityCore";
    
    [JsonIgnore]
    public string AttributesClassName => $"{prefixedDomain}Attributes".ToValidCSharpPascalCase();

    [JsonIgnore]
    public string EntitiesForDomainClassName => $"{Domain}Entities".ToValidCSharpPascalCase();

    [JsonIgnore]
    public Type? AttributesBaseClass { get; set; }
};

record EntityMetaData(string id, string? friendlyName, string cSharpName);

record EntityAttributeMetaData(string JsonName, string CSharpName, Type ClrType);