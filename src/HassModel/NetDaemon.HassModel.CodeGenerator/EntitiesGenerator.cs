using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using NetDaemon.Client.HomeAssistant.Model;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NetDaemon.HassModel.CodeGenerator;

internal static class EntitiesGenerator
{
    public static IEnumerable<MemberDeclarationSyntax> Generate(CodeGenerationSettings codeGenerationSettings, IReadOnlyList<HassState> entities)
    {
        var entitySets = entities.GroupBy(e => (domain: EntityIdHelper.GetDomain(e.EntityId), isNumeric: IsNumeric(e)))
            .Select(g => new EntitySet(g.Key.domain, g.Key.isNumeric, g))
            .OrderBy(s => s.Domain)
            .ToList();

        var entityIds = entities.Select(x => x.EntityId).ToList();

        var entityDomains = GetDomainsFromEntities(entityIds).OrderBy(s => s).ToList();

        yield return GenerateRootEntitiesInterface(entityDomains);

        yield return GenerateRootEntitiesClass(entitySets);

        foreach (var entityClass in entitySets.GroupBy(s => s.EntitiesForDomainClassName).Select(g => GenerateEntitiesForDomainClass(g.Key, g)))
        {
            yield return entityClass;
        }
        
        foreach (var entitySet in entitySets)
        {
            yield return GenerateEntityType(entitySet);
            
            var attrGen = new AttributeTypeGenerator(codeGenerationSettings, entitySet);
            yield return attrGen.GenerateAttributeRecord();
        }
    }
    
    private static TypeDeclarationSyntax GenerateRootEntitiesInterface(IEnumerable<string> domains)
    {
        var autoProperties = domains.Select(domain =>
        {
            var typeName = GetEntitiesForDomainClassName(domain);
            var propertyName = domain.ToPascalCase();

            return (MemberDeclarationSyntax)Property(typeName, propertyName, init: false);
        }).ToArray();

        return Interface("IEntities").AddMembers(autoProperties).ToPublic();
    }

    // The Entities class that provides properties to all Domains
    private static TypeDeclarationSyntax GenerateRootEntitiesClass(IEnumerable<EntitySet> entitySet)
    {
        var haContextNames = GetNames<IHaContext>();

        var properties = entitySet.DistinctBy(s => s.Domain).Select(set =>
        {
            var entitiesTypeName = GetEntitiesForDomainClassName(set.Domain);
            var entitiesPropertyName = set.Domain.ToPascalCase();

            return (MemberDeclarationSyntax)ParseProperty($"{entitiesTypeName} {entitiesPropertyName} => new(_{haContextNames.VariableName});")
                .ToPublic();
        }).ToArray();

        return ClassWithInjected<IHaContext>("Entities")
            .ToPublic()
            .AddModifiers(Token(SyntaxKind.PartialKeyword))
            .WithBase((string)"IEntities").AddMembers(properties);
    }

    private record DomainEntityState(HassState Entity, string ClassName);

    /// <summary>
    /// Generates the class with all the properties for the Entities of one domain
    /// </summary>
    private static TypeDeclarationSyntax GenerateEntitiesForDomainClass(string className, IEnumerable<EntitySet> entitySets)
    {
        entitySets = entitySets.ToList();

        //if there are multiple entitySets, we use the Entity base class since NumericSensor does not inherit from Sensor
        var enumerableBaseClass = entitySets.Count() > 1 ? "Entity" : entitySets.First().EntityClassName;

        var entityClass = ClassWithInjected<IHaContext>(className)
            .ToPublic()
            .WithBase($"IEnumerable<{enumerableBaseClass}>")
            .AddModifiers(Token(SyntaxKind.PartialKeyword));

        var entityStates = entitySets.SelectMany(s=> s.EntityStates.Select(e => new DomainEntityState(e, s.EntityClassName))).ToArray();

        var entityProperties = entityStates.Select(GenerateEntityProperty).ToArray();
        entityClass = entityClass.AddMembers(entityProperties);

        var genericEnumerableMethod = GenerateEnumerableMethod(entityStates, enumerableBaseClass);
        entityClass = entityClass.AddMembers(genericEnumerableMethod);

        entityClass = entityClass.AddMembers(ParseMethod($"IEnumerator<{enumerableBaseClass}> GetEnumerator() => GetEntities().GetEnumerator();").ToPublic());
        entityClass = entityClass.AddMembers(ParseExplicitMethod("IEnumerator IEnumerable.GetEnumerator() => GetEntities().GetEnumerator();"));

        return entityClass;
    }

    private static MemberDeclarationSyntax GenerateEntityProperty(DomainEntityState state)
    {
        var (entity, className) = state;

        var propertyCode = $@"{className} {GetPascalCaseName(entity)} => new(_{GetNames<IHaContext>().VariableName}, ""{entity.EntityId}"");";

        var name = entity.AttributesAs<attributes>()?.friendly_name;
        return ParseProperty(propertyCode).ToPublic().WithSummaryComment(name);
    }

    private static MemberDeclarationSyntax GenerateEnumerableMethod(IEnumerable<DomainEntityState> states, string enumeratorBaseClass)
    {
        StringBuilder sourceBuilder = new();

        sourceBuilder.Append(CultureInfo.InvariantCulture, $"IEnumerable<{enumeratorBaseClass}> GetEntities() {{");

        foreach (var state in states)
        {
            var pascalCaseEntityName = GetPascalCaseName(state.Entity);
            sourceBuilder.Append(CultureInfo.InvariantCulture, $"yield return {pascalCaseEntityName};");
        }

        sourceBuilder.Append('}');

        var enumerableMethod = ParseMethod(sourceBuilder.ToString());

        return enumerableMethod.ToPublic().WithSummaryComment("Get all of the entities in the domain");
    }

    private static string GetPascalCaseName(HassState entity)
    {
        var entityName = EntityIdHelper.GetEntity(entity.EntityId);
        var pascalCaseEntityName = entityName.ToNormalizedPascalCase("E_");
        return pascalCaseEntityName;
    }

    private record attributes(string friendly_name);

    private static bool IsNumeric(HassState entity)
    {
        var domain = EntityIdHelper.GetDomain(entity.EntityId);
        if (EntityIdHelper.NumericDomains.Contains(domain)) return true;

        // Mixed domains have both numeric and non-numeric entities, if it has a 'unit_of_measurement' we threat it as numeric
        return EntityIdHelper.MixedDomains.Contains(domain) && entity.Attributes?.ContainsKey("unit_of_measurement") == true;
    }

    /// <summary>
    /// Generates a record derived from Entity like ClimateEntity or SensorEntity for a specific set of entities
    /// </summary>
    private static TypeDeclarationSyntax GenerateEntityType(EntitySet entitySet)
    {
        string attributesGeneric = entitySet.AttributesClassName;

        var baseType = entitySet.IsNumeric ? typeof(NumericEntity) : typeof(Entity);
        var entityStateType = entitySet.IsNumeric ? typeof(NumericEntityState) : typeof(EntityState);

        var baseClass = $"{SimplifyTypeName(baseType)}<{entitySet.EntityClassName}, {SimplifyTypeName(entityStateType)}<{attributesGeneric}>, {attributesGeneric}>";

        var (className, variableName) = GetNames<IHaContext>();
        var classDeclaration = $@"record {entitySet.EntityClassName} : {baseClass}
                                    {{
                                            public {entitySet.EntityClassName}({className} {variableName}, string entityId) : base({variableName}, entityId)
                                            {{}}

                                            public {entitySet.EntityClassName}({SimplifyTypeName(typeof(Entity))} entity) : base(entity)
                                            {{}}
                                    }}";

        return ParseRecord(classDeclaration)
            .ToPublic()
            .AddModifiers(Token(SyntaxKind.PartialKeyword));
    }

    /// <summary>
    ///     Returns a list of domains from all entities
    /// </summary>
    /// <param name="entities">A list of entities</param>
    private static IEnumerable<string> GetDomainsFromEntities(IEnumerable<string> entities) => entities.Select(EntityIdHelper.GetDomain).Distinct();
}