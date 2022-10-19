using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NetDaemon.HassModel.CodeGenerator;

internal static class Generator
{
    public static IEnumerable<MemberDeclarationSyntax> GenerateTypes(
        IReadOnlyCollection<EntityDomainMetadata> domains,
        IReadOnlyCollection<HassServiceDomain> services)
    {
        var orderedServiceDomains = services.OrderBy(x => x.Domain).ToArray();

        var entityClasses = EntitiesGenerator.Generate(domains);
        var serviceClasses = ServicesGenerator.Generate(orderedServiceDomains);
        var extensionMethodClasses = ExtensionMethodsGenerator.Generate(orderedServiceDomains, domains);

        return new[] { entityClasses, serviceClasses, extensionMethodClasses }.SelectMany(x => x).ToList();
    }

    public static CompilationUnitSyntax BuildCompilationUnit(string namespaceName, params MemberDeclarationSyntax[] generatedTypes)
    {
        return CompilationUnit()
            .AddUsings(UsingNamespaces.Select(u => UsingDirective(ParseName(u))).ToArray())
            .WithLeadingTrivia(TriviaHelper.GetFileHeader()
                .Append(Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true))))
            .AddMembers(FileScopedNamespaceDeclaration(ParseName(namespaceName)))
            .AddMembers(generatedTypes)
            .NormalizeWhitespace();
    }
}