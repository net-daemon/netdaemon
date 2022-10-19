using System.Reflection;
using System.Text.Json.Serialization;
using NetDaemon.HassModel.Entities.Core;

namespace NetDaemon.HassModel.CodeGenerator;

internal static class EntityMetaDataMerger
{
    private static readonly Type[] _possibleBaseTypes = typeof(LightAttributesBase).Assembly.GetTypes();
    
    // We need to merge the previously saved metadata with the current metadata from HA
    // We do this because sometimes entities do not provide all their attributes,
    // like a Light only has a brightness attribute when it is turned on
    // 
    // data structure:
    // [ {
    //   "domain": "weather",
    //   "isNumeric": false,
    //   "entities": [ {
    //       "id": "",
    //       "friendlyName":""
    //   }] 
    //   "attributes" : [ {
    //       "jsonName": "temperature",
    //       "cSharpName": "Temperature",
    //       "clrType": "double"
    //     }]
    // } ]
    //
    // How to merge:
    // Match previous and current EntityDomainMetadata on domain and isnumeric
    //    use all non-matches from both
    //    merge matching items
    //       Match Entities on id
    //           use all non-matches from both
    //           merge matching items
    //               note: current will override previous, in this case that is only the friendlyName which is mainly used for xml documentation and it makes most sense to use the most recent 
    //       Match Attributes on JsonName
    //           use all non-matches from both
    //           merge matching items
    //               Previous attributes keep their Existing CSharpName (assume they are unique)
    //               If types do not match use object
    //       check for duplicate CSharpNames, keep CSharpName fixed for previous

    public static EntitiesMetaData Merge(CodeGenerationSettings codeGenerationSettings, EntitiesMetaData previous, EntitiesMetaData current)
    {
        if (codeGenerationSettings.UseAttributeBaseClasses)
        {
            previous = SetBaseTypes(previous);
            current = SetBaseTypes(current);
        }
        
        return previous with
        {
            Domains = FullOuterJoin(previous.Domains, current.Domains, p => (p.Domain.ToLowerInvariant(), p.IsNumeric), MergeDomains)
                .Select(HandleDuplicateCharpNames)
                .ToList()
        };
    }

    public static EntitiesMetaData SetBaseTypes(EntitiesMetaData entitiesMetaData)
    {
        return entitiesMetaData with
        {
            Domains = entitiesMetaData.Domains.Select(m => DeriveFromBasetype(m, _possibleBaseTypes)).ToList()
        };
    }
    
    private static EntityDomainMetadata DeriveFromBasetype(EntityDomainMetadata domainMetadata, IReadOnlyCollection<Type> possibleBaseTypes)
    {
        var baseType = possibleBaseTypes.FirstOrDefault(t => t.Name == domainMetadata.AttributesClassName + "Base");
        
        var basePropertyJsonNames = baseType?.GetProperties()
            .Select(p => p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name)
            .ToHashSet() ?? new HashSet<string>();

        return domainMetadata with
        {
            AttributesBaseClass = baseType,
            Attributes = domainMetadata.Attributes.Where(a => !basePropertyJsonNames.Contains(a.JsonName)).ToList(),
        };
    }

    private static EntityDomainMetadata MergeDomains(EntityDomainMetadata previous, EntityDomainMetadata current)
    {
        return current with
        {
            Entities = MergeEntitySets(previous.Entities, current.Entities), 
            Attributes = MergeAttributeSets(previous.Attributes, current.Attributes).ToList()
        };
    }

    private static List<EntityMetaData> MergeEntitySets(IEnumerable<EntityMetaData> previous, IEnumerable<EntityMetaData> current)
    {
        // For Entities matching by id => use the current and ignore previous
        return FullOuterJoin(previous, current, e => e.id, (_,c) => c).ToList();
    }

    private static IEnumerable<EntityAttributeMetaData> MergeAttributeSets(
        IReadOnlyCollection<EntityAttributeMetaData> previousAttributes,
        IEnumerable<EntityAttributeMetaData> currentAttributes)
    {
        return FullOuterJoin(previousAttributes, currentAttributes, a => a.JsonName, MergeAttributes);
    }
    
    private static EntityAttributeMetaData MergeAttributes(EntityAttributeMetaData previous, EntityAttributeMetaData current)
    {
        // for Attributes matching by the JsonName keep the previous
        // this makes sure the preferred CSharpName stays the same
        return previous with
        {
            // In case the ClrType derived from the current metadata is not the same as the previous
            // we will use 'object' for safety
            
            ClrType = previous.ClrType == current.ClrType ? previous.ClrType : typeof(object),
            // There is a possible issue here when one of the ClrTypes is 'object'
            // It can be object because either all inputs where JsonValueKind.Null, or there are
            // multiple possible input types.
            // Right here we dont actually know which it was, we will assume there were multiple,
            // so the resulting type will stay object 
        };
    }

    private static EntityDomainMetadata HandleDuplicateCharpNames(EntityDomainMetadata entitiesMetaData)
    {
        // This hashset will initially have all Member names in the base class,
        // We will then also add all new names to this set so we are sure they will all be unique 
        var reservedCSharpNames = entitiesMetaData.AttributesBaseClass?
            .GetMembers().Select(p => p.Name).ToHashSet() ?? new HashSet<string>();

        return entitiesMetaData with
        {
            Attributes = HandleDulicateCSharpNames(reservedCSharpNames, entitiesMetaData.Attributes).ToList()
        };
    }
    
    private static IEnumerable<EntityAttributeMetaData> HandleDulicateCSharpNames(ISet<string> reservedCSharpNames, IReadOnlyCollection<EntityAttributeMetaData> merged)
    {
        return merged
            .GroupBy(t => t.CSharpName)
            .SelectMany(s => DeDuplicateCSharpNames(s.Key, s, reservedCSharpNames));
    }

    private static IEnumerable<EntityAttributeMetaData> DeDuplicateCSharpNames(
        string preferredCSharpName, IEnumerable<EntityAttributeMetaData> items, 
        ISet<string> reservedCSharpNames)
    {
        var list = items.ToList();
        if (list.Count == 1 && reservedCSharpNames.Add(preferredCSharpName))
        {
            // Just one Attribute with this preferredCSharpName AND it was not taken yet 
            return new[] { list[0]};
        }

        // We have duplicates so we apply a suffix to all
        var suffix = 0;
        return list.Select(p => p with { CSharpName = ReserveNextAvailableName() }).ToList();

        string ReserveNextAvailableName()
        {
            string tryName;
            do
            {
                tryName = $"{preferredCSharpName}_{suffix++}";
            } while (!reservedCSharpNames.Add(tryName));

            return tryName;
        }
    }

    /// <summary>
    /// Full outer join two sets based on a key and merge the matches 
    /// </summary>
    /// <returns>
    /// All items from previous and current that dont match
    /// A merged item for all matches based in the Merger delegate
    /// </returns>
    private static IEnumerable<T> FullOuterJoin<T, TKey>(
        IEnumerable<T> previous,
        IEnumerable<T> current, 
        Func<T, TKey> keySelector, 
        Func<T,T,T> merger) where TKey : notnull
    {
        var previousLookup = previous.ToDictionary(keySelector);
        var currentLookup = current.ToLookup(keySelector);
            
        var inPrevious = previousLookup
            .Select(p => (previous: p.Value, current: currentLookup[p.Key].FirstOrDefault()))
            .Select(t => t.current == null 
                ? t.previous // Item in previous doe snot exist in current, return previous 
                : merger(t.previous, t.current)); // match, so call merge delegate
        
        var onlyInCurrent = currentLookup.Where(l => !previousLookup.ContainsKey(l.Key)).SelectMany(l => l);
        return inPrevious.Concat(onlyInCurrent);
    }
}