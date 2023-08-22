using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace NetDaemon.HassModel.CodeGenerator.CodeGeneration;

internal static class HelpersGenerator
{
    public static IEnumerable<MemberDeclarationSyntax> Generate(IReadOnlyCollection<EntityDomainMetadata> domains, IEnumerable<HassServiceDomain> orderedServiceDomains)
    {
        var extensionClass = GenerateServiceCollectionExtension(domains, orderedServiceDomains);
        return new[] { extensionClass };
    }

    /// <summary>
    /// Generates the ServiceCollectionExtensions class
    /// </summary>
    /// public static class GeneratedExtensions
    /// {
    ///    ...
    /// }
    private static ClassDeclarationSyntax GenerateServiceCollectionExtension(IReadOnlyCollection<EntityDomainMetadata> domains, IEnumerable<HassServiceDomain> orderedServiceDomains)
    {
        return
            ClassDeclaration("GeneratedExtensions").WithModifiers(TokenList(Token(PublicKeyword), Token(StaticKeyword)))
                .WithMembers(new SyntaxList<MemberDeclarationSyntax>(new[]
                    {
                        BuildAddHomeAssistantGenerated(domains, orderedServiceDomains)
                    }
                ));
    }

    /// <summary>
    /// Generates the AddHomeAssistantGenerated method
    /// </summary>
    //
    //  public static IServiceCollection AddGeneratedCode(this IServiceCollection serviceCollection)
    //  {
    //      serviceCollection.AddTransient<Entities>();
    //      serviceCollection.AddTransient<AutomationEntities>();
    //      serviceCollection.AddTransient<BinarySensorEntities>();
    //      serviceCollection.AddTransient<Services>();
    //      serviceCollection.AddTransient<AlarmControlPanelServices>();
    //      return serviceCollection;
    // }
    private static MethodDeclarationSyntax BuildAddHomeAssistantGenerated(IEnumerable<EntityDomainMetadata> domains, IEnumerable<HassServiceDomain> orderedServiceDomains)
    {

        var injectableTypes = GetInjectableTypes(domains, orderedServiceDomains);

        var statements = injectableTypes.Select(name =>
            ExpressionStatement(InvocationExpression(
                MemberAccessExpression(SimpleMemberAccessExpression, IdentifierName("serviceCollection"),
                    GenericName(Identifier("AddTransient"))
                        .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(name))))))));

        return MethodDeclaration(IdentifierName("IServiceCollection"), Identifier("AddHomeAssistantGenerated"))
            .WithModifiers(TokenList(Token(PublicKeyword), Token(StaticKeyword)))
            .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier("serviceCollection"))
                .WithModifiers(TokenList(Token(ThisKeyword))).WithType(IdentifierName("IServiceCollection")))))
            .WithBody(Block(
                statements
                    .Append<StatementSyntax>(ReturnStatement(IdentifierName("serviceCollection")))))
            .WithSummaryComment("Registers all injectable generated types in the serviceCollection");
    }

    private static IEnumerable<string> GetInjectableTypes(IEnumerable<EntityDomainMetadata> domains, IEnumerable<HassServiceDomain> orderedServiceDomains) =>
        domains.Select(d => d.EntitiesForDomainClassName)
            .Prepend(EntitiesClassName)
            .Append(ServicesClassName)
            .Union(orderedServiceDomains.Select(d => GetServicesTypeName(d.Domain)));
}
