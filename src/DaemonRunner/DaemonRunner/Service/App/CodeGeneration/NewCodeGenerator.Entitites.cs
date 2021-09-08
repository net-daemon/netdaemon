using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetDaemon.Daemon.Config;
using NetDaemon.Model3.Common;
using NetDaemon.Model3.Entities;
using OldEntityState = NetDaemon.Common.EntityState;

namespace NetDaemon.Service.App.CodeGeneration
{
    public partial class NewCodeGenerator
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
                var typeName = Helpers.NamingHelper.GetEntitiesTypeName(domain);
                var propertyName = domain.ToPascalCase();

                return Helpers.SyntaxFactoryHelper.Property(typeName, propertyName, set: false);
            }).ToArray();

            return Helpers.SyntaxFactoryHelper.ToPublic(Helpers.SyntaxFactoryHelper.Interface("IEntities").AddMembers(autoProperties));
        }

        private static TypeDeclarationSyntax GenerateRootEntitiesClass(IEnumerable<string> domains)
        {
            var haContextNames = Helpers.NamingHelper.GetNames<IHaContext>();

            var properties = domains.Select(domain =>
            {
                var entitiesTypeName = Helpers.NamingHelper.GetEntitiesTypeName(domain);
                var entitiesPropertyName = domain.ToPascalCase();

                return Helpers.SyntaxFactoryHelper.ToPublic(Helpers.SyntaxFactoryHelper.ParseProperty($"{entitiesTypeName} {entitiesPropertyName} => new(_{haContextNames.VariableName});"));
            }).ToArray();

            return Helpers.SyntaxFactoryHelper.ToPublic(Helpers.SyntaxFactoryHelper.WithBase(Helpers.SyntaxFactoryHelper.ClassWithInjected<IHaContext>("Entities"), (string)"IEntities").AddMembers(properties));
        }

        private static IEnumerable<TypeDeclarationSyntax> GenerateEntityAttributeRecords(IEnumerable<OldEntityState> entities)
        {
            foreach (var entityDomainGroups in entities.GroupBy(x => Helpers.EntityIdHelper.GetDomain(x.EntityId)))
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

                        attributes.Add(attributeName, Helpers.TypeHelper.GetType(attributeObject));
                    }
                }

                IEnumerable<(string Name, string TypeName, string SerializationName)> autoPropertiesParams = attributes
                    .Select(a => (Extensions.StringExtensions.ToNormalizedPascalCase(a.Key), Extensions.TypeExtensions.GetFriendlyName(a.Value), a.Key));

                // handles the case when attributes have equal names in PascalCase but different types.
                // i.e. available & Available convert to AvailableString & AvailableBool

                autoPropertiesParams = Extensions.CollectionExtensions.HandleDuplicates(autoPropertiesParams, x => x.Name,
                d => { d.Name = Extensions.StringExtensions.ToNormalizedPascalCase($"{d.Name}{ConfigStringExtensions.ToPascalCase(d.TypeName)}"); return d; });

                // but when they are the same type, we cannot generate meaninguful name so just numerate them.
                // TODO: come up with a meaningful name
                var i = 1;
                autoPropertiesParams = Extensions.CollectionExtensions.HandleDuplicates(autoPropertiesParams, x => x.Name,
                    d => { d.Name += i++; return d; });

                var autoProperties = autoPropertiesParams.Select(a =>
                    Extensions.SyntaxFactoryExtensions.WithAttribute<JsonPropertyNameAttribute>(Helpers.SyntaxFactoryHelper.ToPublic(Helpers.SyntaxFactoryHelper.Property(a.TypeName, a.Name)), a.SerializationName))
                    .ToArray();

                var domain = entityDomainGroups.Key;

                yield return Helpers.SyntaxFactoryHelper.ToPublic(Helpers.SyntaxFactoryHelper.Record(Helpers.NamingHelper.GetAttributesTypeName(domain), autoProperties));
            }
        }

        private static TypeDeclarationSyntax GenerateEntityDomainType(string domain, IEnumerable<string> entities)
        {
            var entityClass = Helpers.SyntaxFactoryHelper.ToPublic(Helpers.SyntaxFactoryHelper.ClassWithInjected<IHaContext>(Helpers.NamingHelper.GetEntitiesTypeName(domain)));

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
            var entityName = Helpers.EntityIdHelper.GetEntity(entityId);

            var propertyCode = $@"{Helpers.NamingHelper.GetDomainEntityTypeName(domain)} {Extensions.StringExtensions.ToNormalizedPascalCase(entityName, (string)"E_")} => new(_{Helpers.NamingHelper.GetNames<IHaContext>().VariableName}, ""{entityId}"");";

            return Helpers.SyntaxFactoryHelper.ToPublic(Helpers.SyntaxFactoryHelper.ParseProperty(propertyCode));
        }

        private static TypeDeclarationSyntax GenerateEntityType(string domain)
        {
            string attributesGeneric = Helpers.NamingHelper.GetAttributesTypeName(domain);

            var entityClass = $"{Helpers.NamingHelper.GetDomainEntityTypeName(domain)}";

            var baseClass = $"{typeof(Entity).FullName}<{entityClass}, {typeof(EntityState).FullName}<string, {attributesGeneric}>, string, {attributesGeneric}>";

            var (className, variableName) = Helpers.NamingHelper.GetNames<IHaContext>();
            var classDeclaration = $@"record {entityClass} : {baseClass}
                                    {{
                                            public {domain.ToPascalCase()}Entity({className} {variableName}, string entityId) : base({variableName}, entityId)
                                            {{
                                            }}
                                    }}";

            return Helpers.SyntaxFactoryHelper.ToPublic(Helpers.SyntaxFactoryHelper.ParseRecord(classDeclaration));
        }
        /// <summary>
        ///     Returns a list of domains from all entities
        /// </summary>
        /// <param name="entities">A list of entities</param>
        internal static IEnumerable<string> GetDomainsFromEntities(IEnumerable<string> entities) => entities.Select(Helpers.EntityIdHelper.GetDomain).Distinct();
    }
}