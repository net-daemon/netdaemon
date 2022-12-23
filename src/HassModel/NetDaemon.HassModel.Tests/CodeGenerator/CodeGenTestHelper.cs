using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.HassModel.CodeGenerator;
using NetDaemon.HassModel.CodeGenerator.Model;

namespace NetDaemon.HassModel.Tests.CodeGenerator;

internal class CodeGenTestHelper
{
    public static CompilationUnitSyntax GenerateCompilationUnit(
        CodeGenerationSettings codeGenerationSettings, 
        IReadOnlyCollection<HassState> states, 
        IReadOnlyCollection<HassServiceDomain> services)
    {
        var metaData = EntityMetaDataGenerator.GetEntityDomainMetaData(states);
        metaData = EntityMetaDataMerger.Merge(codeGenerationSettings, new EntitiesMetaData(), metaData);
        var generatedTypes = Generator.GenerateTypes(metaData.Domains, services).ToArray();
        return Generator.BuildCompilationUnit(codeGenerationSettings.Namespace, generatedTypes);
        
    }

    public static void AssertCodeCompiles(string generated,  string appCode)
    {
        var syntaxtrees = new []
        {
            SyntaxFactory.ParseSyntaxTree(generated, path: "generated.cs"),
            SyntaxFactory.ParseSyntaxTree(appCode, path: "appcode.cs")
        };
        var _ = typeof(IServiceCollection); // make sure this type is not removed

        var compilation = CSharpCompilation.Create("tempAssembly",
            syntaxtrees,
            AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).Select(a => MetadataReference.CreateFromFile(a.Location)).ToArray(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable)
        );

        var emitResult = compilation.Emit(Stream.Null);

        var warningsOrErrors = emitResult.Diagnostics
            .Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning).ToList();

        if (!warningsOrErrors.Any()) return;
        
        var msg = new StringBuilder("Compile of generated code failed.\r\n");
        foreach (var diagnostic in warningsOrErrors)
        {
            msg.AppendLine(diagnostic.ToString());
        }

        msg.AppendLine();
        msg.AppendLine("generated.cs");
        
        // output the generated code including line numbers to help debugging 
        var linesWithNumbers = generated.Split(new [] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Select((l, i) => $"{i+1,5}: {l}");
        
        msg.AppendJoin(Environment.NewLine, linesWithNumbers);
            
        Assert.Fail(msg.ToString());
    }
}