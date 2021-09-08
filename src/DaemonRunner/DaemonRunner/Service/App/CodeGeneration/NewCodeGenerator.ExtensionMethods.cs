using System.Collections.Generic;
using System.Linq;
using JoySoftware.HomeAssistant.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OldEntityState = NetDaemon.Common.EntityState;

namespace NetDaemon.Service.App.CodeGeneration
{
    public partial class NewCodeGenerator
    {
        private static IEnumerable<ClassDeclarationSyntax> GenerateExtensionMethodClasses(IEnumerable<HassServiceDomain> serviceDomains, IEnumerable<OldEntityState> entities)
        {
            var entityDomains = entities.GroupBy(e => Helpers.EntityIdHelper.GetDomain(e.EntityId)).Select(x => x.Key);

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
            var serviceTypeDeclaration = Helpers.SyntaxFactoryHelper.ToStatic(Helpers.SyntaxFactoryHelper.ToPublic(Helpers.SyntaxFactoryHelper.Class(Helpers.NamingHelper.GetEntityDomainExtensionMethodClassName(domain))));

            var serviceMethodDeclarations = services
                .SelectMany(service => GenerateExtensionMethod(domain, service))
                .ToArray();

            return serviceTypeDeclaration.AddMembers(serviceMethodDeclarations);
        }

        private static IEnumerable<MemberDeclarationSyntax> GenerateExtensionMethod(string domain, HassService service)
        {
            var serviceName = service.Service!;

            var args = GetServiceArguments(domain, service);

            var entityTypeName = Helpers.NamingHelper.GetDomainEntityTypeName(domain);

            yield return Helpers.SyntaxFactoryHelper.ToStatic(Helpers.SyntaxFactoryHelper.ToPublic(Helpers.SyntaxFactoryHelper.ParseMethod(
                $@"void {Helpers.NamingHelper.GetServiceMethodName(serviceName)}(this {entityTypeName} entity {(args is not null ? $", {args.GetParametersString()}" : string.Empty)})
            {{
                entity.CallService(""{serviceName}""{(args is not null ? $", {ServiceArguments.GetParametersVariable()}" : string.Empty)});
            }}")));

            if (args is not null)
            {
                yield return Helpers.SyntaxFactoryHelper.ToStatic(Helpers.SyntaxFactoryHelper.ToPublic(Helpers.SyntaxFactoryHelper.ParseMethod(
                    $@"void {Helpers.NamingHelper.GetServiceMethodName(serviceName)}(this {entityTypeName} entity , {args.GetParametersDecomposedString()})
                {{
                    entity.CallService(""{serviceName}"", {args.GetParametersDecomposedVariable()});
                }}")));
            }
        }
    }
}