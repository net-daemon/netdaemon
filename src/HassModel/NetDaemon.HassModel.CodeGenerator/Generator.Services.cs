using System.Collections.Generic;
using System.Linq;
using JoySoftware.HomeAssistant.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetDaemon.HassModel.CodeGenerator.Extensions;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;
using static NetDaemon.HassModel.CodeGenerator.Helpers.NamingHelper;
using static NetDaemon.HassModel.CodeGenerator.Helpers.SyntaxFactoryHelper;
namespace NetDaemon.HassModel.CodeGenerator
{
    public partial class Generator
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
            var haContextNames = GetNames<IHaContext>();
            var properties = domains.Select(domain =>
            {
                var propertyCode = $"{GetServicesTypeName(domain)} {domain.ToPascalCase()} => new(_{haContextNames.VariableName});";

                return ParseProperty(propertyCode).ToPublic();
            }).ToArray();

            return ClassWithInjected<IHaContext>("Services").WithBase((string)"IServices").AddMembers(properties).ToPublic();
        }

        private static TypeDeclarationSyntax GenerateRootServicesInterface(IEnumerable<string> domains)
        {
            var properties = domains.Select(domain =>
            {
                var typeName = GetServicesTypeName(domain);
                var domainName = domain.ToPascalCase();

                return Property(typeName, domainName, init: false);
            }).ToArray();

            return Interface("IServices").AddMembers(properties).ToPublic();
        }

        private static TypeDeclarationSyntax GenerateServicesDomainType(string domain, IEnumerable<HassService> services)
        {
            var serviceTypeDeclaration = ClassWithInjected<IHaContext>(GetServicesTypeName(domain)).ToPublic();

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
                .Select(argument => Property($"{argument.TypeName!}?", argument.PropertyName!).ToPublic()
                    .WithJsonPropertyName(argument.HaName!).WithSummaryComment(argument.Comment))
                .ToArray();

            yield return Record(serviceArguments.TypeName, autoProperties).ToPublic();
        }

        private static IEnumerable<MemberDeclarationSyntax> GenerateServiceMethod(string domain, HassService service)
        {
            var serviceName = service.Service!;

            var serviceArguments = GetServiceArguments(domain, service);
            var haContextVariableName = GetVariableName<IHaContext>("_");

            var argsParametersString = serviceArguments is not null ? $"{serviceArguments.TypeName} data" : null ;

            var serviceMethodName = GetServiceMethodName(serviceName);
            var targetParam = service.Target is not null ? $"{typeof(ServiceTarget).FullName} target" : null;
            var targetArg = service.Target is not null ? "target" : "null";
            var targetComment = service.Target is not null ? ParameterComment("target", "The target for this service call") : (SyntaxTrivia?)null;

            // method using arguments object 
            yield return ParseMethod(
                $@"void {serviceMethodName}({JoinArguments(targetParam, argsParametersString)})
            {{
                {haContextVariableName}.CallService({JoinArguments($"\"{domain}\"", $"\"{serviceName}\"", targetArg, serviceArguments is not null ? "data" : null)});
            }}").ToPublic()
                .WithSummaryComment(service.Description)
                .AppendTrivia(targetComment);
            
            if (serviceArguments is not null)
            {
                // method using arguments as separate parameters 
                yield return ParseMethod(
                $@"void {serviceMethodName}({JoinArguments(targetParam, serviceArguments.GetParametersDecomposedString())})
            {{
                {haContextVariableName}.CallService({JoinArguments($"\"{domain}\"", $"\"{serviceName}\"", targetArg, serviceArguments.GetParametersDecomposedVariable())});
            }}")
                    .ToPublic()
                    .WithSummaryComment(service.Description)
                    .AppendTrivia(targetComment)
                    .WithParameterComments(serviceArguments);
            }
        }

        private static string JoinArguments(params string?[] args) => string.Join(", ", args.Where(s => !string.IsNullOrEmpty(s)));
        
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
