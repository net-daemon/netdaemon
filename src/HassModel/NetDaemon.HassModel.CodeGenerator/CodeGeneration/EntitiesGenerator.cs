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
            yield return GenerateEntitiesForDomainClass(domainMetadata.Key, [.. domainMetadata]);
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

            return (MemberDeclarationSyntax)AutoPropertyGet(typeName, propertyName);
        });

        return InterfaceDeclaration("IEntities").WithMembers(List(autoProperties)).ToPublic();
    }

    // The Entities class that provides properties to all Domains
    private static TypeDeclarationSyntax GenerateRootEntitiesClass(IEnumerable<EntityDomainMetadata> domains)
    {
        var properties = domains.DistinctBy(s => s.Domain).Select(set =>
        {
            var entitiesTypeName = GetEntitiesForDomainClassName(set.Domain);
            var entitiesPropertyName = set.Domain.ToPascalCase();

            return PropertyWithExpressionBodyNew(entitiesTypeName, entitiesPropertyName, "_haContext");
        }).ToArray();

        return ClassWithInjectedHaContext(EntitiesClassName)
            .WithBase("IEntities")
            .AddMembers(properties);
    }

    /// <summary>
    /// Generates the class with all the properties for the Entities of one domain
    /// </summary>
    private static TypeDeclarationSyntax GenerateEntitiesForDomainClass(string className, IList<EntityDomainMetadata> entitySets)
    {
        var entityClass = ClassWithInjectedHaContext(className);

        entityClass = entityClass.AddMembers(EnumerateAllGenerator.GenerateEnumerateMethods(entitySets[0].Domain, entitySets[0].EntityClassName));

        var entityProperty = entitySets.SelectMany(s=>s.Entities.Select(e => GenerateEntityProperty(e, s.EntityClassName))).ToArray();

        return entityClass.AddMembers(entityProperty);
    }

    private static MemberDeclarationSyntax GenerateEntityProperty(EntityMetaData entity, string className)
    {
        return PropertyWithExpressionBodyNew(className, entity.cSharpName, "_haContext", $"\"{entity.id}\"")
            .WithSummaryComment(entity.friendlyName);
    }

    /// <summary>
    /// Generates a record derived from Entity like ClimateEntity or SensorEntity for a specific set of entities
    /// </summary>
    private static MemberDeclarationSyntax GenerateEntityType(EntityDomainMetadata domainMetaData)
    {
        var attributesGeneric = domainMetaData.AttributesClassName;

        var baseType = domainMetaData.IsNumeric ? typeof(NumericEntity) : typeof(Entity);
        var entityStateType = domainMetaData.IsNumeric ? typeof(NumericEntityState) : typeof(EntityState);
        var baseClass = $"{SimplifyTypeName(baseType)}<{domainMetaData.EntityClassName}, {SimplifyTypeName(entityStateType)}<{attributesGeneric}>, {attributesGeneric}>";

        var coreinterface = domainMetaData.CoreInterfaceName;
        if (coreinterface != null)
        {
            baseClass += $", {coreinterface}";
        }

        var (className, variableName) = GetNames<IHaContext>();
        var classDeclaration = $$"""
            record {{domainMetaData.EntityClassName}} : {{baseClass}}
            {
                public {{domainMetaData.EntityClassName}}({{className}} {{variableName}}, string entityId) : base({{variableName}}, entityId)
                {}

                public {{domainMetaData.EntityClassName}}({{SimplifyTypeName(typeof(IEntityCore))}} entity) : base(entity)
                {}
            }
            """;

        return ParseMemberDeclaration(classDeclaration)!
            .ToPublic()
            .AddModifiers(Token(SyntaxKind.PartialKeyword));
    }
}
