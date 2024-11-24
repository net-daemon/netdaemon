using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Castle.Core.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.HassModel.CodeGenerator;
using NetDaemon.HassModel.CodeGenerator.Model;

namespace NetDaemon.HassModel.Tests.CodeGenerator;

internal static class CodeGenTestHelper
{
    public static CompilationUnitSyntax GenerateCompilationUnit(
        CodeGenerationSettings codeGenerationSettings,
        IReadOnlyCollection<HassState> states,
        IReadOnlyCollection<HassServiceDomain>? services = null)
    {
        var metaData = EntityMetaDataGenerator.GetEntityDomainMetaData(states);
        metaData = EntityMetaDataMerger.Merge(codeGenerationSettings, new EntitiesMetaData(), metaData);

        var generatedTypes = Generator.GenerateTypes(metaData.Domains, services ?? []).ToArray();

        return Generator.BuildCompilationUnit(codeGenerationSettings.Namespace, generatedTypes);
    }

    public static void AssertCodeCompiles(string generated, string appCode)
    {
        var compilation = CreateCSharpCompilation(generated, appCode);
        var emitResult = compilation.Emit(Stream.Null); // we are not actually interested in the result, just check for errors or warnings

        AssertNoErrorsAndWarnings(emitResult, generated);
    }

    public static object RunApp(string generated, [StringSyntax("C#")] string appCode, IServiceCollection serviceCollection, AssemblyLoadContext context)
    {
        var assembly = LoadDynamicAssembly(generated, appCode, context);
        assembly.GetType("HomeAssistantGenerated.GeneratedExtensions")!.GetMethod("AddHomeAssistantGenerated")!.Invoke(null, [serviceCollection]);

        var provider = serviceCollection.BuildServiceProvider();

        var appType = assembly.GetTypes().Single(t => t.GetAttribute<NetDaemonTestAppAttribute>() is not null);
        return ActivatorUtilities.CreateInstance(provider, appType);
    }

    public static Assembly LoadDynamicAssembly(string generated, string appCode, AssemblyLoadContext context)
    {
        var compilation = CreateCSharpCompilation(generated, appCode);

        using var peStream = new MemoryStream();
        using var symStream = new MemoryStream();

        var emitResult = compilation.Emit(peStream, symStream);

        AssertNoErrorsAndWarnings(emitResult, generated);

        peStream.Seek(0, SeekOrigin.Begin);
        symStream.Seek(0, SeekOrigin.Begin);
        return context.LoadFromStream(peStream, symStream);
    }

    // We lazy load the references and cache them static because when we start loading new assemblies
    // we do not want to also reference those in the subsequent test
    private static readonly Lazy<PortableExecutableReference[]> LazyReferences = new(() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location)).ToArray());

    private static CSharpCompilation CreateCSharpCompilation(string generated, string appCode)
    {
        var syntaxTrees = new[]
        {
            SyntaxFactory.ParseSyntaxTree(generated, path: "generated.cs", encoding:Encoding.UTF8),
            SyntaxFactory.ParseSyntaxTree(appCode, path: "appcode.cs", encoding:Encoding.UTF8)
        };

        var compilation = CSharpCompilation.Create("TestCodeAssembly",
            syntaxTrees,
            LazyReferences.Value,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable)
        );
        return compilation;
    }

    private static void AssertNoErrorsAndWarnings(EmitResult emitResult, string generatedCode)
    {
        HashSet<string> ignoreWarning = ["CS0618", "CS1701"];

        var warningsOrErrors = emitResult.Diagnostics
            .Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning
                        && !ignoreWarning.Contains(d.Id)).ToList();

        if (warningsOrErrors is [])
        {
            emitResult.Success.Should().BeTrue();
            return;
        }

        var msg = new StringBuilder("Compile of generated code failed.\r\n");
        foreach (var diagnostic in warningsOrErrors)
        {
            msg.AppendLine(diagnostic.ToString());
        }

        msg.AppendLine();
        msg.AppendLine("generated.cs");

        // output the generated code including line numbers to help debugging
        var linesWithNumbers = generatedCode.Split(["\r\n", "\r", "\n"], StringSplitOptions.None)
            .Select((l, i) => $"{i + 1,5}: {l}");

        msg.AppendJoin(Environment.NewLine, linesWithNumbers);

        Assert.Fail(msg.ToString());
    }
}
