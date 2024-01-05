using NetDaemon.Client.HomeAssistant.Model;

namespace NetDaemon.HassModel.CodeGenerator;

internal static class EntityMetaDataGenerator
{
    /// <summary>
    /// Creates metadata describing entities and their attributes based on all the states from HA 
    /// </summary>
    public static EntitiesMetaData GetEntityDomainMetaData(IReadOnlyCollection<HassState> entityStates)
    {
        // We need to group the entities by domain, because some domains (Sensor) have numeric and non numeric
        // entities that should be treated differently we also group by IsNumeric
        var domainGroups = entityStates.GroupBy(e => (domain: EntityIdHelper.GetDomain(e.EntityId), isNumeric: IsNumeric(e)));

        return new EntitiesMetaData{Domains = domainGroups.OrderBy(g => g.Key)
            .Select(mapEntityDomainMetadata)
            .ToList()};
    }

    private static EntityDomainMetadata mapEntityDomainMetadata(IGrouping<(string domain, bool isNumeric), HassState> domainGroup) =>
        new (
            Domain: domainGroup.Key.domain, 
            IsNumeric: domainGroup.Key.isNumeric, 
            Entities: MapToEntityMetaData(domainGroup),
            Attributes: AttributeMetaDataGenerator.GetMetaDataFromEntityStates(domainGroup).ToList());

    private static List<EntityMetaData> MapToEntityMetaData(IEnumerable<HassState> g)
    {
        var entityMetaDatas = g.Select(state => new EntityMetaData(
            id: state.EntityId, 
            friendlyName: GetFriendlyName(state),
            cSharpName: GetPreferredCSharpName(state.EntityId)));

        entityMetaDatas = DeDuplicateCSharpNames(entityMetaDatas);
        
        return entityMetaDatas.OrderBy(e => e.id).ToList();
    }

    private static IEnumerable<EntityMetaData> DeDuplicateCSharpNames(IEnumerable<EntityMetaData> entityMetaDatas)
    {
        // The PascalCased EntityId might not be unique because we removed all underscores
        // If we have duplicates we will use the original ID instead and only make sure it is a Valid C# identifier
        return entityMetaDatas
            .ToLookup(e => e.cSharpName)
            .SelectMany(e => e.Count() == 1 
                ? e 
                : e.Select(i => i with { cSharpName = GetUniqueCSharpName(i.id) }));
    }

    /// <summary>
    /// We prefer the Property names for Entities to be the id in PascalCase
    /// </summary>
    private static string GetPreferredCSharpName(string id) => EntityIdHelper.GetEntity(id).ToValidCSharpPascalCase();

    /// <summary>
    /// HA entity ID's can only contain [a-z0-9_]. Which are all also valid in Csharp identifiers.
    /// HA does allow the id to begin with a digit which is not valid for C#. In those cases it will be prefixed with
    /// an _ 
    /// </summary>
    private static string GetUniqueCSharpName(string id) => EntityIdHelper.GetEntity(id).ToValidCSharpIdentifier();

    private static string? GetFriendlyName(HassState hassState) => hassState.AttributesAs<Attributes>()?.friendly_name;

    private record Attributes(string friendly_name);

    private static bool IsNumeric(HassState entity)
    {
        var domain = EntityIdHelper.GetDomain(entity.EntityId);
        if (EntityIdHelper.NumericDomains.Contains(domain)) return true;

        // Mixed domains have both numeric and non-numeric entities, if it has a 'unit_of_measurement' we threat it as numeric
        return EntityIdHelper.MixedDomains.Contains(domain) && entity.Attributes?.ContainsKey("unit_of_measurement") == true;
    }
}
