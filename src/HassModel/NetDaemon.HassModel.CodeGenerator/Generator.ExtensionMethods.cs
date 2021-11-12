using System.Collections.Generic;
using System.Linq;
using JoySoftware.HomeAssistant.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetDaemon.HassModel.CodeGenerator.Helpers;
using static NetDaemon.HassModel.CodeGenerator.Helpers.NamingHelper;
using static NetDaemon.HassModel.CodeGenerator.Helpers.SyntaxFactoryHelper;

namespace NetDaemon.HassModel.CodeGenerator
{
    public partial class Generator
    {
        private static IEnumerable<ClassDeclarationSyntax> GenerateExtensionMethodClasses(IEnumerable<HassServiceDomain> serviceDomains, IReadOnlyCollection<HassState> entities)
        {
            var entityDomains = entities.GroupBy(e => EntityIdHelper.GetDomain(e.EntityId)).Select(x => x.Key);

            foreach (var domainServicesGroup in serviceDomains
                .Where(sd =>
                    sd.Services?.Any() == true
                    && sd.Services.Any(s => entityDomains.Contains(s.Target?.Entity?.Domain)))
                .GroupBy(x => x.Domain, x => x.Services))
            {
                var domain = domainServicesGroup.Key!;
                var domainServices = domainServicesGroup
                    .SelectMany(services => services!)
                    .Where(s => s.Target?.Entity?.Domain != null)
                    .Select(group => group)
                    .OrderBy(x => x.Service)
                    .ToList();

                yield return GenerateDomainExtensionClass(domain, domainServices);
            }
        }

        private static ClassDeclarationSyntax GenerateDomainExtensionClass(string domain, IEnumerable<HassService> services)
        {
            var serviceTypeDeclaration = Class(GetEntityDomainExtensionMethodClassName(domain)).ToPublic().ToStatic();

            var serviceMethodDeclarations = services
                .SelectMany(service => GenerateExtensionMethod(domain, service))
                .ToArray();

            return serviceTypeDeclaration.AddMembers(serviceMethodDeclarations);
        }

        private static IEnumerable<MemberDeclarationSyntax> GenerateExtensionMethod(string domain, HassService service)
        {
            var serviceName = service.Service!;

            var serviceArguments = GetServiceArguments(domain, service);

            var entityTypeName = GetDomainEntityTypeName(service.Target?.Entity?.Domain!);

            yield return ParseMethod(
                $@"void {GetServiceMethodName(serviceName)}(this {entityTypeName} entity {(serviceArguments is not null ? $", {serviceArguments.GetParametersString()}" : string.Empty)})
            {{
                entity.CallService(""{serviceName}""{(serviceArguments is not null ? $", {serviceArguments.GetParametersVariable()}" : string.Empty)});
            }}").ToPublic().ToStatic();

            if (serviceArguments is not null)
            {
                yield return ParseMethod(
                    $@"void {GetServiceMethodName(serviceName)}(this {entityTypeName} entity, {serviceArguments.GetParametersDecomposedString()})
                {{
                    entity.CallService(""{serviceName}"", {serviceArguments.GetParametersDecomposedVariable()});
                }}").ToPublic().ToStatic();
            }
        }
    }
}