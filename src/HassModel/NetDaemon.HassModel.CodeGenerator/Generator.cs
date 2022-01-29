using System.Runtime.CompilerServices;
using NetDaemon.Client.Common.HomeAssistant.Model;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.HassModel.CodeGenerator;

internal static class Generator
{
    public static string GenerateCode(string nameSpace, IReadOnlyCollection<HassState> entities, IReadOnlyCollection<HassServiceDomain> services)
    {
        var code = CreateCompilationUnitSyntax(nameSpace, entities, services);
        return code.ToFullString();
    }

    internal static CompilationUnitSyntax CreateCompilationUnitSyntax(string nameSpace, IReadOnlyCollection<HassState> entities, IReadOnlyCollection<HassServiceDomain> services)
    {
        var orderedEntities = entities.OrderBy(x => x.EntityId).ToArray();
        var orderedServiceDomains = services.OrderBy(x => x.Domain).ToArray();

        var code = CompilationUnit()
            .AddUsings(UsingDirective(ParseName("System")))
            .AddUsings(UsingDirective(ParseName("System.Collections.Generic")))
            .AddUsings(UsingNamespaces.OrderBy(s => s).Select(u => UsingDirective(ParseName(u))).ToArray());

        var namespaceDeclaration = NamespaceDeclaration(ParseName(nameSpace)).NormalizeWhitespace();

        namespaceDeclaration = namespaceDeclaration.AddMembers(EntitiesGenerator.Generate(orderedEntities).ToArray());
        namespaceDeclaration = namespaceDeclaration.AddMembers(ServicesGenerator.Generate(orderedServiceDomains).ToArray());
        namespaceDeclaration = namespaceDeclaration.AddMembers(ExtensionMethodsGenerator.Generate(orderedServiceDomains, entities).ToArray());

        code = code.AddMembers(namespaceDeclaration);

        code = code.NormalizeWhitespace(Tab.ToString(), eol: "\n");

        return code;
    }
}
