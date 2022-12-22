using Microsoft.CodeAnalysis.CSharp;

namespace NetDaemon.HassModel.CodeGenerator;

/// <summary>
/// Generates classes with extension methods for calling services on Entities
/// </summary>
/// <example>
/// public static class InputButtonEntityExtensionMethods
/// {
///     ///<summary>Press the input button entity.</summary>
///     public static void Press(this InputButtonEntity target)
///     {
///         target.CallService("press");
///     }
/// 
///     ///<summary>Press the input button entity.</summary>
///     public static void Press(this IEnumerable<InputButtonEntity> target)
///     {
///         target.CallService("press");
///     }
/// }
/// </example>
internal static class ExtensionMethodsGenerator
{
    public static IEnumerable<MemberDeclarationSyntax> Generate(IEnumerable<HassServiceDomain> serviceDomains, IReadOnlyCollection<EntityDomainMetadata> entityDomains)
    {
        var entityClassNameByDomain = entityDomains.ToLookup(e => e.Domain, e => e.EntityClassName);
        
        return serviceDomains
            .Select(sd => GenerateDomainExtensionClass(sd, entityClassNameByDomain))
            .OfType<MemberDeclarationSyntax>(); // filter out nulls
    }

    private static ClassDeclarationSyntax? GenerateDomainExtensionClass(HassServiceDomain serviceDomain, ILookup<string, string> entityClassNameByDomain)
    {
        var serviceMethodDeclarations = serviceDomain.Services
            .OrderBy(x => x.Service)
            .SelectMany(service => GenerateExtensionMethods(serviceDomain.Domain, service, entityClassNameByDomain))
            .ToArray();

        if (!serviceMethodDeclarations.Any()) return null;

        return SyntaxFactory.ClassDeclaration(GetEntityDomainExtensionMethodClassName(serviceDomain.Domain))
                .AddMembers(serviceMethodDeclarations)
                .ToPublic()
                .ToStatic();
    }

    private static IEnumerable<MemberDeclarationSyntax> GenerateExtensionMethods(string domain, HassService service, ILookup<string, string> entityClassNameByDomain)
    {
        var targetEntityDomain = service.Target?.Entity?.Domain;
        if (targetEntityDomain == null) yield break;
        
        var entityTypeName = entityClassNameByDomain[targetEntityDomain].FirstOrDefault();
        if (entityTypeName == null) yield break;
        
        var serviceName = service.Service;
        var serviceArguments = ServiceArguments.Create(domain, service);
        var enumerableTargetTypeName = $"IEnumerable<{entityTypeName}>";

        if (serviceArguments is null)
        {
            yield return ExtensionMethodWithoutArguments(service, serviceName, entityTypeName);
            yield return ExtensionMethodWithoutArguments(service, serviceName, enumerableTargetTypeName);
        }
        else
        {
            yield return ExtensionMethodWithClassArgument(service, serviceName, entityTypeName, serviceArguments);
            yield return ExtensionMethodWithClassArgument(service, serviceName, enumerableTargetTypeName, serviceArguments);

            yield return ExtensionMethodWithSeparateArguments(service, serviceName, entityTypeName, serviceArguments);
            yield return ExtensionMethodWithSeparateArguments(service, serviceName, enumerableTargetTypeName, serviceArguments);
        }
    }

    private static GlobalStatementSyntax ExtensionMethodWithoutArguments(HassService service, string serviceName, string entityTypeName)
    {
        return ParseMethod(
                $@"void {GetServiceMethodName(serviceName)}(this {entityTypeName} target)
                {{
                    target.CallService(""{serviceName}"");
                }}")
            .ToPublic()
            .ToStatic()
            .WithSummaryComment(service.Description);
    }
    
    private static GlobalStatementSyntax ExtensionMethodWithClassArgument(HassService service, string serviceName, string entityTypeName, ServiceArguments serviceArguments)
    {
        return ParseMethod(
                $@"void {GetServiceMethodName(serviceName)}(this {entityTypeName} target, {serviceArguments.TypeName} data)
                {{
                    target.CallService(""{serviceName}"", data);
                }}")
            .ToPublic()
            .ToStatic()
            .WithSummaryComment(service.Description);
    }
    
    private static MemberDeclarationSyntax ExtensionMethodWithSeparateArguments(HassService service, string serviceName, string entityTypeName, ServiceArguments serviceArguments)
    {
        return ParseMethod(
                $@"void {GetServiceMethodName(serviceName)}(this {entityTypeName} target, {serviceArguments.GetParametersList()})
                {{
                    target.CallService(""{serviceName}"", {serviceArguments.GetNewServiceArgumentsTypeExpression()});
                }}")
            .ToPublic()
            .ToStatic()
            .WithSummaryComment(service.Description)
            .WithParameterComment("target", $"The {entityTypeName} to call this service for")
            .WithParameterComments(serviceArguments);
    }
}
