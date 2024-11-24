using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NetDaemon.HassModel.CodeGenerator;

/// <summary>
/// Generates classes with extension methods for calling services on Entities
/// </summary>
/// <example>
/// <![CDATA[
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
/// ]]>
/// </example>
internal static class ExtensionMethodsGenerator
{
    public static IEnumerable<MemberDeclarationSyntax> Generate(IEnumerable<HassServiceDomain> serviceDomains, IReadOnlyCollection<EntityDomainMetadata> entityDomains)
    {
        var entityClassNameByDomain = entityDomains.ToLookup(e => e.Domain, e => e.CoreInterfaceName ?? e.EntityClassName);

        return serviceDomains
            .Select(sd => GenerateDomainExtensionClass(sd, entityClassNameByDomain))
            .OfType<MemberDeclarationSyntax>(); // filter out nulls
    }

    private static ClassDeclarationSyntax? GenerateDomainExtensionClass(HassServiceDomain serviceDomain, ILookup<string, string> entityClassNameByDomain)
    {
        var serviceMethodDeclarations = serviceDomain.Services
            .OrderBy(x => x.Service)
            .SelectMany(service => GenerateExtensionMethodsForService(serviceDomain.Domain, service, entityClassNameByDomain))
            .ToArray();

        return serviceMethodDeclarations.Length == 0
            ? null
            : ClassDeclaration(GetEntityDomainExtensionMethodClassName(serviceDomain.Domain))
                .AddMembers(serviceMethodDeclarations)
                .ToPublic()
                .ToStatic();
    }

    private static IEnumerable<MemberDeclarationSyntax> GenerateExtensionMethodsForService(string domain, HassService service, ILookup<string, string> entityClassNameByDomain)
    {
        // There can be multiple Target Domains, so generate methods for each
        var targetEntityDomains = service.Target?.Entity.SelectMany(e => e.Domain) ?? [];

        return targetEntityDomains.SelectMany(targetEntityDomain => GenerateExtensionMethodsForService(domain, service, targetEntityDomain, entityClassNameByDomain));
    }

    private static IEnumerable<MemberDeclarationSyntax> GenerateExtensionMethodsForService(string domain, HassService service, string targetEntityDomain, ILookup<string, string> entityClassNameByDomain)
    {
        var entityTypeName = entityClassNameByDomain[targetEntityDomain].FirstOrDefault();

        if (entityTypeName == null) yield break;

        var serviceName = service.Service;
        var serviceArguments = ServiceArguments.Create(domain, service);
        var enumerableTargetTypeName = $"IEnumerable<{entityTypeName}>";

        if (serviceArguments is null)
        {
            if (service.Response is not null)
            {
                yield return ExtensionMethodWithoutArguments(service, serviceName, entityTypeName, true);
                // No support for IEnumerable<Entity> with async methods for now may be added later
            }
            yield return ExtensionMethodWithoutArguments(service, serviceName, entityTypeName, false);
            yield return ExtensionMethodWithoutArguments(service, serviceName, enumerableTargetTypeName, false);
        }
        else
        {
            if (service.Response is not null)
            {
                yield return ExtensionMethodWithClassArgument(service, serviceName, entityTypeName, serviceArguments, true);
                yield return ExtensionMethodWithSeparateArguments(service, serviceName, entityTypeName, serviceArguments, true);
                // No support for IEnumerable<Entity> with async methods now may be added later
            }
            yield return ExtensionMethodWithClassArgument(service, serviceName, entityTypeName, serviceArguments, false);
            yield return ExtensionMethodWithClassArgument(service, serviceName, enumerableTargetTypeName, serviceArguments, false);

            yield return ExtensionMethodWithSeparateArguments(service, serviceName, entityTypeName, serviceArguments, false);
            yield return ExtensionMethodWithSeparateArguments(service, serviceName, enumerableTargetTypeName, serviceArguments, false);
        }
    }

    private static MemberDeclarationSyntax ExtensionMethodWithoutArguments(HassService service, string serviceName, string entityTypeName, bool generateAsync)
    {
        if (generateAsync)
        {
            return ParseMemberDeclaration($$"""
                    public static Task<JsonElement?> {{GetServiceMethodName(serviceName)}}Async(this {{entityTypeName}} target, object? data = null)
                    {
                        return target.CallServiceWithResponseAsync("{{serviceName}}", data);
                    }
                    """)!
                .WithSummaryComment(service.Description);
        }
        else
        {
            return ParseMemberDeclaration($$"""
                    public static void {{GetServiceMethodName(serviceName)}}(this {{entityTypeName}} target, object? data = null)
                    {
                        target.CallService("{{serviceName}}", data);
                    }
                    """)!
                .WithSummaryComment(service.Description);
        }
    }

    private static MemberDeclarationSyntax ExtensionMethodWithClassArgument(HassService service, string serviceName, string entityTypeName,
            ServiceArguments serviceArguments, bool isAsync)
    {
        if (isAsync)
        {
            return ParseMemberDeclaration($$"""
                    public static Task<JsonElement?> {{GetServiceMethodName(serviceName)}}Async(this {{entityTypeName}} target, {{serviceArguments.TypeName}} data)
                    {
                        return target.CallServiceWithResponseAsync("{{serviceName}}", data);
                    }
                    """)!
                .WithSummaryComment(service.Description);
        }
        else
        {
            return ParseMemberDeclaration($$"""
                    public static void {{GetServiceMethodName(serviceName)}}(this {{entityTypeName}} target, {{serviceArguments.TypeName}} data)
                    {
                        target.CallService("{{serviceName}}", data);
                    }
                    """)!
                .WithSummaryComment(service.Description);
        }
    }

    static MemberDeclarationSyntax ExtensionMethodWithSeparateArguments(HassService service, string serviceName, string entityTypeName,
            ServiceArguments serviceArguments, bool isAsync)
    {
        if (isAsync)
        {
            return ParseMemberDeclaration($$"""
                    public static Task<JsonElement?> {{GetServiceMethodName(serviceName)}}Async(this {{entityTypeName}} target, {{serviceArguments.GetParametersList()}})
                    {
                        return target.CallServiceWithResponseAsync("{{serviceName}}", {{serviceArguments.GetNewServiceArgumentsTypeExpression()}});
                    }
                    """)!
                .WithSummaryComment(service.Description)
                .AppendParameterComment("target", $"The {entityTypeName} to call this service for")
                .AppendParameterComments(serviceArguments);
        }
        else
        {
            return ParseMemberDeclaration($$"""
                    public static void {{GetServiceMethodName(serviceName)}}(this {{entityTypeName}} target, {{serviceArguments.GetParametersList()}})
                    {
                        target.CallService("{{serviceName}}", {{serviceArguments.GetNewServiceArgumentsTypeExpression()}});
                    }
                    """)!
                .WithSummaryComment(service.Description)
                .AppendParameterComment("target", $"The {entityTypeName} to call this service for")
                .AppendParameterComments(serviceArguments);
        }
    }
}
