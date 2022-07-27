﻿using System.Runtime.CompilerServices;
using NetDaemon.Client.HomeAssistant.Model;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.HassModel.CodeGenerator;

internal static class Generator
{
    public static string GenerateCode(CodeGenerationSettings codeGenerationSettings, IReadOnlyCollection<HassState> entities, IReadOnlyCollection<HassServiceDomain> services)
    { 
        var code = CreateCompilationUnitSyntax(codeGenerationSettings, entities, services);
        return code.ToFullString();
    }

    public static IEnumerable<CompilationUnitSyntax> GenerateCodePerEntity(CodeGenerationSettings codeGenerationSettings, IReadOnlyCollection<HassState> entities, IReadOnlyCollection<HassServiceDomain> services)
    {
        var code = CreateCompilationUnitSyntaxPerFile(codeGenerationSettings, entities, services);
        return code;
    }

    internal static IEnumerable<CompilationUnitSyntax> CreateCompilationUnitSyntaxPerFile(CodeGenerationSettings codeGenerationSettings, IReadOnlyCollection<HassState> entities, IReadOnlyCollection<HassServiceDomain> services)
    {
        var units = new List<CompilationUnitSyntax>();
        var classes = new List<MemberDeclarationSyntax>();
        var orderedEntities = entities.OrderBy(x => x.EntityId).ToArray();
        var orderedServiceDomains = services.OrderBy(x => x.Domain).ToArray();

        classes.AddRange(EntitiesGenerator.Generate(codeGenerationSettings, orderedEntities).ToArray());
        classes.AddRange(ServicesGenerator.Generate(orderedServiceDomains).ToArray());
        classes.AddRange(ExtensionMethodsGenerator.Generate(orderedServiceDomains, entities).ToArray());

        classes.ForEach(x =>
        {
            units.Add(CompilationUnit()
                .AddUsings(UsingDirective(ParseName("System")))
                .AddUsings(UsingDirective(ParseName("System.Collections.Generic")))
                .AddUsings(UsingNamespaces.OrderBy(s => s).Select(u => UsingDirective(ParseName(u))).ToArray())
                .AddMembers(NamespaceDeclaration(ParseName(codeGenerationSettings.Namespace)).NormalizeWhitespace().AddMembers(x))
                .NormalizeWhitespace(Tab.ToString(), eol: "\n"));
        });

        return units;
    }

    internal static CompilationUnitSyntax CreateCompilationUnitSyntax(CodeGenerationSettings codeGenerationSettings, IReadOnlyCollection<HassState> entities, IReadOnlyCollection<HassServiceDomain> services)
    {
        var orderedEntities = entities.OrderBy(x => x.EntityId).ToArray();
        var orderedServiceDomains = services.OrderBy(x => x.Domain).ToArray();

        var code = CompilationUnit()
            .AddUsings(UsingDirective(ParseName("System")))
            .AddUsings(UsingDirective(ParseName("System.Collections.Generic")))
            .AddUsings(UsingNamespaces.OrderBy(s => s).Select(u => UsingDirective(ParseName(u))).ToArray());

        var namespaceDeclaration = NamespaceDeclaration(ParseName(codeGenerationSettings.Namespace)).NormalizeWhitespace();

        namespaceDeclaration = namespaceDeclaration.AddMembers(EntitiesGenerator.Generate(codeGenerationSettings, orderedEntities).ToArray());
        namespaceDeclaration = namespaceDeclaration.AddMembers(ServicesGenerator.Generate(orderedServiceDomains).ToArray());
        namespaceDeclaration = namespaceDeclaration.AddMembers(ExtensionMethodsGenerator.Generate(orderedServiceDomains, entities).ToArray());

        code = code.AddMembers(namespaceDeclaration);

        code = code.NormalizeWhitespace(Tab.ToString(), eol: "\n");

        return code;
    }
}