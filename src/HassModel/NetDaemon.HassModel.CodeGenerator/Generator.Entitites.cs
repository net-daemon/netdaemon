using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetDaemon.HassModel.CodeGenerator.Helpers;
using NetDaemon.HassModel.CodeGenerator.Extensions;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;
using static NetDaemon.HassModel.CodeGenerator.Helpers.NamingHelper;
using static NetDaemon.HassModel.CodeGenerator.Helpers.SyntaxFactoryHelper;
using OldEntityState = NetDaemon.Common.EntityState;

namespace NetDaemon.HassModel.CodeGenerator
{
    public partial class Generator
    {
        private static IEnumerable<TypeDeclarationSyntax> GenerateEntityTypes(IEnumerable<OldEntityState> entities)
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

                return Property(typeName, propertyName, init: false);
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

                return ParseProperty($"{entitiesTypeName} {entitiesPropertyName} => new(_{haContextNames.VariableName});").ToPublic();
            }).ToArray();

            return ClassWithInjected<IHaContext>("Entities").WithBase((string)"IEntities").AddMembers(properties).ToPublic();
        }

        private static IEnumerable<TypeDeclarationSyntax> GenerateEntityAttributeRecords(IEnumerable<OldEntityState> entities)
        {
            foreach (var entityDomainGroups in entities.GroupBy(x => EntityIdHelper.GetDomain(x.EntityId)))
            {
                var attributes = new Dictionary<string, Type>();

                foreach (var entity in entityDomainGroups)
                {
                    foreach (var (attributeName, attributeObject) in new Dictionary<string, object>(entity.Attribute))
                    {
                        if (attributes.ContainsKey(attributeName))
                        {
                            continue;
                        }

                        attributes.Add(attributeName, TypeHelper.GetType(attributeObject));
                    }
                }

                IEnumerable<(string Name, string TypeName, string SerializationName)> autoPropertiesParams = attributes
                    .Select(a => (a.Key.ToNormalizedPascalCase(), a.Value.GetFriendlyName(), a.Key));

                // handles the case when attributes have equal names in PascalCase but different types.
                // i.e. available & Available convert to AvailableString & AvailableBool

                autoPropertiesParams = autoPropertiesParams.HandleDuplicates(x => x.Name,
                d => { d.Name = $"{d.Name}{d.TypeName.ToPascalCase()}".ToNormalizedPascalCase(); return d; });

                // but when they are the same type, we cannot generate meaninguful name so just numerate them.
                // TODO: come up with a meaningful name
                var i = 1;
                autoPropertiesParams = autoPropertiesParams.HandleDuplicates(x => x.Name,
                    d => { d.Name += i++; return d; });

                var autoProperties = autoPropertiesParams.Select(a =>
                    Property(a.TypeName, a.Name).ToPublic().WithAttribute<JsonPropertyNameAttribute>(a.SerializationName))
                    .ToArray();

                var domain = entityDomainGroups.Key;

                yield return Record(GetAttributesTypeName(domain), autoProperties).ToPublic();
            }
        }

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

        private static PropertyDeclarationSyntax GenerateEntityProperty(string entityId, string domain)
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
        internal static IEnumerable<string> GetDomainsFromEntities(IEnumerable<string> entities) => entities.Select(EntityIdHelper.GetDomain).Distinct();
    }
}