using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using JoySoftware.HomeAssistant.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.HassModel.CodeGenerator;

[SuppressMessage("ReSharper", "CoVariantArrayConversion")]
public static partial class Generator
{
    public static string GenerateCode(string nameSpace, IReadOnlyCollection<HassState> entities, IReadOnlyCollection<HassServiceDomain> services)
    {
        var code = CreateCompilationUnitSyntax(nameSpace, entities, services);
            return code.ToFullString();
    }

    internal static CompilationUnitSyntax CreateCompilationUnitSyntax(string nameSpace, IReadOnlyCollection<HassState> entities, IReadOnlyCollection<HassServiceDomain> services)
    {
        var orderedEntities = entities.OrderBy(x => x.EntityId);

        var code = CompilationUnit()
            .AddUsings(UsingDirective(ParseName("System")))
                .AddUsings(UsingDirective(ParseName("System.Collections.Generic")));

        var namespaceDeclaration = NamespaceDeclaration(ParseName(nameSpace)).NormalizeWhitespace();

        namespaceDeclaration = namespaceDeclaration.AddMembers(EntitiesGenerator.Generate(orderedEntities.ToList()).ToArray());
        namespaceDeclaration = namespaceDeclaration.AddMembers(ServicesGenerator.Generate(services.OrderBy(x => x.Domain)).ToArray());
        namespaceDeclaration = namespaceDeclaration.AddMembers(ExtensionMethodsGenerator.Generate(services.OrderBy(x => x.Domain), entities).ToArray());

        code = code.AddMembers(namespaceDeclaration);

        code = code.NormalizeWhitespace(Tab.ToString(), eol: "\n");
        
        return code;
    }
}