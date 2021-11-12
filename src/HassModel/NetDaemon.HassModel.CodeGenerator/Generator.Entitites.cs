using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using JoySoftware.HomeAssistant.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetDaemon.HassModel.CodeGenerator.Helpers;
using NetDaemon.HassModel.CodeGenerator.Extensions;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;
using static NetDaemon.HassModel.CodeGenerator.Helpers.NamingHelper;
using static NetDaemon.HassModel.CodeGenerator.Helpers.SyntaxFactoryHelper;

namespace NetDaemon.HassModel.CodeGenerator
{
    public partial class Generator
    {
        private static IEnumerable<TypeDeclarationSyntax> GenerateEntityTypes(IReadOnlyCollection<HassState> entities)
        {
            var entityIds = entities.Select(x => x.EntityId).ToList();

            var entityDomains = GetDomainsFromEntities(entityIds).OrderBy(s => s).ToList();

            yield return GenerateRootEntitiesInterface(entityDomains);

            yield return GenerateRootEntitiesClass(entityDomains);

            foreach (var entityClass in entityDomains.Select(entityDomain => GenerateEntityDomainType(entityDomain, entityIds)))
            {
                yield return entityClass;
            }

            foreach (var entityDomain in entityDomains)
            {
                yield return GenerateEntityType(entityDomain);
            }

            foreach (var attributeRecord in GenerateEntityAttributeRecords(entities))
            {
                yield return attributeRecord;
            }
        }
        private static TypeDeclarationSyntax GenerateRootEntitiesInterface(IEnumerable<string> domains)
        {
            var autoProperties = domains.Select(domain =>
            {
                var typeName = GetEntitiesTypeName(domain);
                var propertyName = domain.ToPascalCase();

                return (MemberDeclarationSyntax)Property(typeName, propertyName, init: false);
            }).ToArray();

            return Interface("IEntities").AddMembers(autoProperties).ToPublic();
        }

        private static TypeDeclarationSyntax GenerateRootEntitiesClass(IEnumerable<string> domains)
        {
            var haContextNames = GetNames<IHaContext>();

            var properties = domains.Select(domain =>
            {
                var entitiesTypeName = GetEntitiesTypeName(domain);
                var entitiesPropertyName = domain.ToPascalCase();

                return (MemberDeclarationSyntax)ParseProperty($"{entitiesTypeName} {entitiesPropertyName} => new(_{haContextNames.VariableName});")
                    .ToPublic();
            }).ToArray();

            return ClassWithInjected<IHaContext>("Entities").WithBase((string)"IEntities").AddMembers(properties).ToPublic();
        }

        private static IEnumerable<TypeDeclarationSyntax> GenerateEntityAttributeRecords(IEnumerable<HassState> entities)
        {
            // group the entities by their domain and create one attribute record for each domain
            return entities.GroupBy(x => EntityIdHelper.GetDomain(x.EntityId))
                .Select(entityDomainGroup => GenerateAtributeRecord(entityDomainGroup.Key, entityDomainGroup));
        }

        private static RecordDeclarationSyntax GenerateAtributeRecord(string domainName, IEnumerable<HassState> entityStates)
        {
            // Get all attributes of all entities in this set
            var jsonProperties = entityStates.SelectMany(s => s.AttributesJson?.EnumerateObject() ?? Enumerable.Empty<JsonProperty>());
            
            // Group the attributes by JsonPropertyName and find the best ClrType that fits all
            var attributesByJsonName = jsonProperties
                .GroupBy(p => p.Name)
                .Select(group => (CSharpName: group.Key.ToNormalizedPascalCase(), 
                                  JsonName: group.Key, 
                                  Type: GetBestClrType(group)));
            
            // We might have different json names that after CamelCasing result in the same CSharpName 
            var uniqueProperties = attributesByJsonName
                .GroupBy(t => t.CSharpName)
                .SelectMany(DeduplictateCSharpName)
                .OrderBy(p => p.CSharpName);
            
            var propertyDeclarations = uniqueProperties.Select(a => Property($"{a.ClrType.GetFriendlyName()}?", a.CSharpName)
                                                                    .ToPublic()
                                                                    .WithAttribute<JsonPropertyNameAttribute>(a.JsonName));

            return Record(GetAttributesTypeName(domainName), propertyDeclarations).ToPublic();
        }

        private static IEnumerable<(string CSharpName, string JsonName, Type ClrType)> DeduplictateCSharpName(IEnumerable<(string CSharpName, string JsonName, Type ClrType)> items)
        {
            var list = items.ToList();
            if (list.Count == 1) return new[] { list.First() };

            return list.OrderBy(i => i.JsonName).Select((p, i) => ($"{p.CSharpName}_{i}", jsonName: p.JsonName, type: p.ClrType));
        }

        private static Type GetBestClrType(IEnumerable<JsonProperty> valueKinds)
        {
            var distinctCrlTypes = valueKinds
                .Select(p => p.Value.ValueKind)
                .Distinct()
                .Where(k => k!= JsonValueKind.Null) // null fits in any type so we can ignore it for now
                .Select(MapJsonType)
                .ToHashSet();

            // If all have the same clr type use that, if not it will be 'object'
            return distinctCrlTypes.Count == 1 
                ? distinctCrlTypes.First() 
                : typeof(object); 
        }
        
        private static Type MapJsonType(JsonValueKind kind) =>
            kind switch
            {
                JsonValueKind.False => typeof(bool),
                JsonValueKind.Undefined => typeof(object),
                JsonValueKind.Object => typeof(object),
                JsonValueKind.Array => typeof(object),
                JsonValueKind.String => typeof(string),
                JsonValueKind.Number => typeof(double),
                JsonValueKind.True => typeof(bool),
                JsonValueKind.Null => typeof(object),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };

        private static TypeDeclarationSyntax GenerateEntityDomainType(string domain, IEnumerable<string> entities)
        {
            var entityClass = ClassWithInjected<IHaContext>(GetEntitiesTypeName(domain)).ToPublic();

            var domainEntities = entities.Where(EntityIsOfDomain(domain)).ToList();

            var entityProperty = domainEntities.Select(entityId => GenerateEntityProperty(entityId, domain)).ToArray();

            return entityClass.AddMembers(entityProperty);
        }

        private static Func<string, bool> EntityIsOfDomain(string domain)
        {
            return n => n.StartsWith(domain + ".", StringComparison.InvariantCultureIgnoreCase);
        }

        private static MemberDeclarationSyntax GenerateEntityProperty(string entityId, string domain)
        {
            var entityName = EntityIdHelper.GetEntity(entityId);

            var propertyCode = $@"{GetDomainEntityTypeName(domain)} {entityName.ToNormalizedPascalCase((string)"E_")} => new(_{GetNames<IHaContext>().VariableName}, ""{entityId}"");";

            return ParseProperty(propertyCode).ToPublic();
        }

        private static TypeDeclarationSyntax GenerateEntityType(string domain)
        {
            string attributesGeneric = GetAttributesTypeName(domain);

            var entityClass = $"{GetDomainEntityTypeName(domain)}";

            var baseClass = $"{typeof(Entity).FullName}<{entityClass}, {typeof(EntityState).FullName}<{attributesGeneric}>, {attributesGeneric}>";

            var (className, variableName) = GetNames<IHaContext>();
            var classDeclaration = $@"record {entityClass} : {baseClass}
                                    {{
                                            public {domain.ToPascalCase()}Entity({className} {variableName}, string entityId) : base({variableName}, entityId)
                                            {{
                                            }}
                                    }}";

            return ParseRecord(classDeclaration).ToPublic();
        }
        /// <summary>
        ///     Returns a list of domains from all entities
        /// </summary>
        /// <param name="entities">A list of entities</param>
        private static IEnumerable<string> GetDomainsFromEntities(IEnumerable<string> entities) => entities.Select(EntityIdHelper.GetDomain).Distinct();
    }
}
