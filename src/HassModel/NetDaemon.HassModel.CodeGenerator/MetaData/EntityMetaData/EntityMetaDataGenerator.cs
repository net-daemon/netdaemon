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

    private static List<EntityMetaData> MapToEntityMetaData(IEnumerable<HassState> g) =>
        g.Select(state => new EntityMetaData(state.EntityId, GetFriendlyName(state)))
            .OrderBy(s=>s.id).ToList();

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
