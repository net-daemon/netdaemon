using System.Collections.Generic;
using System.Linq;
using JoySoftware.HomeAssistant.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetDaemon.Daemon.Config;
using NetDaemon.Model3.Common;
namespace NetDaemon.Service.App.CodeGeneration
{
    public partial class NewCodeGenerator
    {
        private static IEnumerable<TypeDeclarationSyntax> GenerateServiceTypes(IEnumerable<HassServiceDomain> serviceDomains)
        {
            var domains = serviceDomains.Select(x => x.Domain!);

            yield return GenerateRootServicesInterface(domains);

            yield return GenerateRootServicesType(domains);

            foreach (var domainServicesGroup in serviceDomains.Where(sd => sd.Services?.Any() == true).GroupBy(x => x.Domain, x => x.Services))
            {
                var domain = domainServicesGroup.Key!;
                var domainServices = domainServicesGroup
                    .SelectMany(services => services!)
                    .Select(group => group)
                    .OrderBy(x => x.Service)
                    .ToList();

                yield return GenerateServicesDomainType(domain, domainServices);

                foreach (var domainService in domainServices)
                {
                    foreach (var serviceArgsRecord in GenerateServiceArgsRecord(domain, domainService))
                    {
                        yield return serviceArgsRecord;
                    }
                }
            }
        }

        private static TypeDeclarationSyntax GenerateRootServicesType(IEnumerable<string> domains)
        {
            var haContextNames = Helpers.NamingHelper.GetNames<IHaContext>();
            var properties = domains.Select(domain =>
            {
                var propertyCode = $"{Helpers.NamingHelper.GetServicesTypeName(domain)} {domain.ToPascalCase()} => new(_{haContextNames.VariableName});";

                return Helpers.SyntaxFactoryHelper.ToPublic(Helpers.SyntaxFactoryHelper.ParseProperty(propertyCode));
            }).ToArray();

            return Helpers.SyntaxFactoryHelper.ToPublic(Helpers.SyntaxFactoryHelper.WithBase(Helpers.SyntaxFactoryHelper.ClassWithInjected<IHaContext>("Services"), (string)"IServices").AddMembers(properties));
        }

        private static TypeDeclarationSyntax GenerateRootServicesInterface(IEnumerable<string> domains)
        {
            var properties = domains.Select(domain =>
            {
                var typeName = Helpers.NamingHelper.GetServicesTypeName(domain);
                var domainName = domain.ToPascalCase();

                return Helpers.SyntaxFactoryHelper.Property(typeName, domainName, set: false);
            }).ToArray();

            return Helpers.SyntaxFactoryHelper.ToPublic(Helpers.SyntaxFactoryHelper.Interface("IServices").AddMembers(properties));
        }

        private static TypeDeclarationSyntax GenerateServicesDomainType(string domain, IEnumerable<HassService> services)
        {
            var serviceTypeDeclaration = Helpers.SyntaxFactoryHelper.ToPublic(Helpers.SyntaxFactoryHelper.ClassWithInjected<IHaContext>(Helpers.NamingHelper.GetServicesTypeName(domain)));

            var serviceMethodDeclarations = services.SelectMany(service => GenerateServiceMethod(domain, service)).ToArray();

            return serviceTypeDeclaration.AddMembers(serviceMethodDeclarations);
        }

        private static IEnumerable<TypeDeclarationSyntax> GenerateServiceArgsRecord(string domain, HassService service)
        {
            var serviceArguments = GetServiceArguments(domain, service);

            if (serviceArguments is null)
            {
                yield break;
            }

            var autoProperties = serviceArguments.Arguments
                .Select(argument => Helpers.SyntaxFactoryHelper.Property(argument.TypeName!, argument.PropertyName!))
                .ToArray();

            yield return Helpers.SyntaxFactoryHelper.ToPublic(Helpers.SyntaxFactoryHelper.Record(serviceArguments.TypeName, autoProperties));
        }

        private static IEnumerable<MemberDeclarationSyntax> GenerateServiceMethod(string domain, HassService service)
        {
            var serviceName = service.Service!;

            var serviceArguments = GetServiceArguments(domain, service);
            var haContextVariableName = Helpers.NamingHelper.GetVariableName<IHaContext>("_");

            var argsParametersString = serviceArguments is not null ? $"{serviceArguments.TypeName} data {(serviceArguments.HasRequiredArguments ? "" : "= null")}" : null ;

            if (service.Target is not null)
            {
                yield return Helpers.SyntaxFactoryHelper.ToPublic(Helpers.SyntaxFactoryHelper.ParseMethod(
                    $@"void {Helpers.NamingHelper.GetServiceMethodName(serviceName)}({typeof(HassTarget).FullName} target {(argsParametersString is not null ? "," : "")} {argsParametersString})
                {{
                    {haContextVariableName}.CallService(""{domain}"", ""{serviceName}"", target {(serviceArguments is not null ? ", data" : string.Empty)});
                }}"));
            }
            else
            {
                yield return Helpers.SyntaxFactoryHelper.ToPublic(Helpers.SyntaxFactoryHelper.ParseMethod(
                    $@"void {Helpers.NamingHelper.GetServiceMethodName(serviceName)}({argsParametersString})
                {{
                    {haContextVariableName}.CallService(""{domain}"", ""{serviceName}"" {(serviceArguments is not null ? ", null" : "")} {(serviceArguments is not null ? ", data" : "")});
                }}"));
            }
        }

        private static ServiceArguments? GetServiceArguments(string domain, HassService service)
        {
            if (service.Fields is null || service.Fields.Count == 0)
            {
                return null;
            }

            return new ServiceArguments(domain, service.Service!, service.Fields);
        }
    }
}