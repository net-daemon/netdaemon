using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace NetDaemon.AppModel.Internal.Compiler;

internal record CompiledAssemblyResult(CollectibleAssemblyLoadContext AssemblyContext, Assembly CompiledAssembly);

internal class Compiler : ICompiler
{
    private readonly ISyntaxTreeResolver _syntaxResolver;
    private readonly ILogger<Compiler> _logger;

    public Compiler(
        ISyntaxTreeResolver syntaxResolver,
        ILogger<Compiler> logger
        )
    {
        _syntaxResolver = syntaxResolver;
        _logger = logger;
    }

    public CompiledAssemblyResult Compile()
    {
        CollectibleAssemblyLoadContext context = new();
        var compilation = GetSharpCompilation();

        using var peStream = new MemoryStream();
        var emitResult = compilation.Emit(peStream: peStream);

        if (emitResult.Success)
        {
            peStream.Seek(0, SeekOrigin.Begin);
            var assembly = context!.LoadFromStream(peStream);
            return new CompiledAssemblyResult(context, assembly);
        }
        else
        {
            var error = PrettyPrintCompileError(emitResult);

            _logger.LogError("Failed to compile applications\n{error}", error);

            context.Unload();
            // Finally do cleanup and release memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
            throw new InvalidOperationException();
        }
    }

    private CSharpCompilation GetSharpCompilation()
    {
        var syntaxTrees = _syntaxResolver.GetSyntaxTrees();
        var metaDataReference = GetDefaultReferences();

        return CSharpCompilation.Create(
            $"daemon_apps_{Path.GetRandomFileName()}.dll",
            syntaxTrees.ToArray(),
            references: metaDataReference.ToArray(),
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release,
                assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default
            )
        );
    }

    private static IEnumerable<MetadataReference> GetDefaultReferences()
    {
        var metaDataReference = new List<MetadataReference>(10)
                    {
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(System.Text.RegularExpressions.Regex).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(System.ComponentModel.INotifyPropertyChanged).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.DynamicExpression).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Microsoft.Extensions.Logging.Abstractions.NullLogger).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Observable).Assembly.Location),
                    };

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
                metaDataReference.Add(MetadataReference.CreateFromFile(assembly.Location));
        }

        metaDataReference.Add(MetadataReference.CreateFromFile((Assembly.GetEntryAssembly()?.Location)!));

        return metaDataReference;
    }

    private static string PrettyPrintCompileError(EmitResult emitResult)
    {
        var msg = new StringBuilder();

        foreach (var emitResultDiagnostic in emitResult.Diagnostics)
        {
            if (emitResultDiagnostic.Severity == DiagnosticSeverity.Error)
            {
                msg.AppendLine(emitResultDiagnostic.ToString());
            }
        }

        return msg.ToString();
    }

    public void Dispose()
    {
    }
}