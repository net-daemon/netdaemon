using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp;
using NetDaemon.HassModel.CodeGenerator.CodeGeneration;

namespace NetDaemon.HassModel.CodeGenerator;

internal static class EntitiesGenerator
{
    public static IEnumerable<MemberDeclarationSyntax> Generate(IReadOnlyCollection<EntityDomainMetadata> metaData)
    {
        var entityDomains = metaData.Select(d => d.Domain).Distinct();

        yield return GenerateRootEntitiesInterface(entityDomains);

        yield return GenerateRootEntitiesClass(metaData);

        foreach (var domainMetadata in metaData.GroupBy(m => m.EntitiesForDomainClassName))
        {
            yield return GenerateEntiesForDomainClass(domainMetadata.Key, domainMetadata);
        }
        
        foreach (var domainMetadata in metaData)
        {
            yield return GenerateEntityType(domainMetadata);
            
            yield return AttributeTypeGenerator.GenerateAttributeRecord(domainMetadata);
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
    private static TypeDeclarationSyntax GenerateRootEntitiesClass(IEnumerable<EntityDomainMetadata> entitySet)
    {
        var haContextNames = GetNames<IHaContext>();

        var properties = entitySet.DistinctBy(s=>s.Domain).Select(set =>
        {
            var entitiesTypeName = GetEntitiesForDomainClassName(set.Domain);
            var entitiesPropertyName = set.Domain.ToPascalCase();

            return (MemberDeclarationSyntax)ParseProperty($"{entitiesTypeName} {entitiesPropertyName} => new(_{haContextNames.VariableName});")
                .ToPublic();
        }).ToArray();

        return ClassWithInjected<IHaContext>("Entities")
            .ToPublic()
            .AddModifiers(Token(SyntaxKind.PartialKeyword))
            .WithBase((string)"IEntities")
            .AddMembers(properties);
    }

    /// <summary>
    /// Generates the class with all the properties for the Entities of one domain
    /// </summary>
    private static TypeDeclarationSyntax GenerateEntiesForDomainClass(string className, IEnumerable<EntityDomainMetadata> entitySets)
    {
        var entityClass = ClassWithInjected<IHaContext>(className)
            .ToPublic()
            .AddModifiers(Token(SyntaxKind.PartialKeyword));

        var entityProperty = entitySets.SelectMany(s=>s.Entities.Select(e => GenerateEntityProperty(e, s.EntityClassName))).ToArray();

        return entityClass.AddMembers(entityProperty);
    }

    private static MemberDeclarationSyntax GenerateEntityProperty(EntityMetaData entity, string className)
    {
        var entityName = EntityIdHelper.GetEntity(entity.id);

        var propertyCode = $@"{className} {entityName.ToNormalizedPascalCase((string)"E_")} => new(_{GetNames<IHaContext>().VariableName}, ""{entity.id}"");";

        var name = entity.friendlyName;
        return ParseProperty(propertyCode).ToPublic().WithSummaryComment(name);
    }

    /// <summary>
    /// Generates a record derived from Entity like ClimateEntity or SensorEntity for a specific set of entities
    /// </summary>
    private static TypeDeclarationSyntax GenerateEntityType(EntityDomainMetadata domainMetaData)
    {
        string attributesGeneric = domainMetaData.AttributesClassName;

        var baseType = domainMetaData.IsNumeric ? typeof(NumericEntity) : typeof(Entity);
        var entityStateType = domainMetaData.IsNumeric ? typeof(NumericEntityState) : typeof(EntityState);

        var baseClass = $"{SimplifyTypeName(baseType)}<{domainMetaData.EntityClassName}, {SimplifyTypeName(entityStateType)}<{attributesGeneric}>, {attributesGeneric}>";

        var (className, variableName) = GetNames<IHaContext>();
        var classDeclaration = $@"record {domainMetaData.EntityClassName} : {baseClass}
                                    {{
                                            public {domainMetaData.EntityClassName}({className} {variableName}, string entityId) : base({variableName}, entityId)
                                            {{}}

                                            public {domainMetaData.EntityClassName}({SimplifyTypeName(typeof(Entity))} entity) : base(entity)
                                            {{}}
                                    }}";

        return ParseRecord(classDeclaration)
            .ToPublic()
            .AddModifiers(Token(SyntaxKind.PartialKeyword));
    }
}