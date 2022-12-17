using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NetDaemon.HassModel.CodeGenerator;

internal static class ServicesGenerator
{
    public static IEnumerable<MemberDeclarationSyntax> Generate(IReadOnlyList<HassServiceDomain> serviceDomains)
    {
        var domains = serviceDomains.Select(x => x.Domain!).ToArray();

        yield return GenerateRootServicesInterface(domains);

        yield return GenerateRootServicesType(domains);

        foreach (var domainServicesGroup in serviceDomains.Where(sd => sd.Services?.Any() == true).GroupBy(x => x.Domain, x => x.Services))
        {
            var domain = domainServicesGroup.Key!;
            var domainServices = domainServicesGroup
                .SelectMany(services => services!)
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
        var serviceArguments = ServiceArguments.Create(domain, service);

        if (serviceArguments is null)
        {
            yield break;
        }

        var autoProperties = serviceArguments.Arguments
            .Select(argument => Property($"{argument.TypeName!}?", argument.PropertyName!).ToPublic()
                .WithJsonPropertyName(argument.HaName!).WithSummaryComment(argument.Comment))
            .ToArray();

        yield return Record(serviceArguments.TypeName, autoProperties)
            .ToPublic()
            .AddModifiers(Token(SyntaxKind.PartialKeyword));
    }

    private static IEnumerable<MemberDeclarationSyntax> GenerateServiceMethod(string domain, HassService service)
    {
        var serviceName = service.Service!;

        var serviceArguments = ServiceArguments.Create(domain, service);
        var haContextVariableName = GetVariableName<IHaContext>("_");

        var serviceMethodName = GetServiceMethodName(serviceName);

        var targetParam = service.Target is not null ? $"{SimplifyTypeName(typeof(ServiceTarget))} target" : null;
        var targetArg = service.Target is not null ? "target" : "null";
        var targetComment = service.Target is not null ? ParameterComment("target", "The target for this service call") : (SyntaxTrivia?)null;

        if (serviceArguments is null)
        {
            // method without arguments
            yield return ParseMethod(
                    $@"void {serviceMethodName}({targetParam})
                    {{
                        {haContextVariableName}.CallService(""{domain}"", ""{serviceName}"", {targetArg});
                    }}")
                .ToPublic()
                .WithSummaryComment(service.Description)
                .AppendTrivia(targetComment);
        }
        else
        {
            // method using arguments object 
            yield return ParseMethod(
                    $@"void {serviceMethodName}({JoinList(targetParam, serviceArguments.TypeName)} data)
                    {{
                        {haContextVariableName}.CallService(""{domain}"", ""{serviceName}"", {targetArg}, data);
                    }}")
                .ToPublic()
                .WithSummaryComment(service.Description)
                .AppendTrivia(targetComment);

            // method using arguments as separate parameters 
            yield return ParseMethod(
                    $@"void {serviceMethodName}({JoinList(targetParam, serviceArguments.GetParametersList())})
                    {{
                        {haContextVariableName}.CallService(""{domain}"", ""{serviceName}"", {targetArg}, {serviceArguments.GetNewServiceArgumentsTypeExpression()});
                    }}")
                .ToPublic()
                .WithSummaryComment(service.Description)
                .AppendTrivia(targetComment)
                .WithParameterComments(serviceArguments);
        }
    }

    private static string JoinList(params string?[] args) => string.Join(", ", args.Where(s => !string.IsNullOrEmpty(s)));
}