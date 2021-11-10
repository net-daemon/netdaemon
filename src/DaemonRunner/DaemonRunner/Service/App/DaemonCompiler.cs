using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;
using NetDaemon.Common.Reactive;
using NetDaemon.Daemon;
using NetDaemon.Infrastructure.Extensions;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.Service.App
{
    public class CollectibleAssemblyLoadContext : AssemblyLoadContext
    {
        public CollectibleAssemblyLoadContext() : base(isCollectible: true)
        {
        }

        protected override Assembly? Load(AssemblyName assemblyName) => null;
    }

    /// <summary>
    ///     Compiles the code into a collectable context
    /// </summary>
    internal static class DaemonCompiler
    {
        public static (IEnumerable<Type>, CollectibleAssemblyLoadContext?) GetDaemonApps(string codeFolder, ILogger logger)
        {
            var loadedApps = new List<Type>(50);

            // Load the compiled apps
            var (compiledApps, compileErrorText) = GetCompiledApps(out CollectibleAssemblyLoadContext alc, codeFolder, logger);

            if (compiledApps is not null)
                loadedApps.AddRange(compiledApps);
            else if (!string.IsNullOrEmpty(compileErrorText))
                logger.LogError(compileErrorText);
            else if (loadedApps.Count == 0)
                logger.LogWarning("No .cs files files found, please add files to netdaemonfolder {CodeFolder}", codeFolder);

            return (loadedApps, alc);
        }

        public static (IEnumerable<Type>?, string) GetCompiledApps(out CollectibleAssemblyLoadContext alc, string codeFolder, ILogger logger)
        {
            var assembly = GetCompiledAppAssembly(out alc, codeFolder, logger);

            if (assembly == null)
                return (null, "Compile error");

            return (assembly.GetAppClasses(), string.Empty);
        }

        public static Assembly GetCompiledAppAssembly(out CollectibleAssemblyLoadContext alc, string codeFolder, ILogger logger)
        {
            alc = new CollectibleAssemblyLoadContext();

            try
            {
                var compilation = GetCsCompilation(codeFolder);

                foreach (var syntaxTree in compilation.SyntaxTrees)
                {
                    if (Path.GetFileName(syntaxTree.FilePath) != "_EntityExtensions.cs")
                        WarnIfExecuteIsMissing(syntaxTree, compilation, logger);

                    InterceptAppInfo(syntaxTree, compilation);
                }

                using var peStream = new MemoryStream();
                var emitResult = compilation.Emit(peStream: peStream);

                if (emitResult.Success)
                {
                    peStream.Seek(0, SeekOrigin.Begin);

                    return alc!.LoadFromStream(peStream);
                }
                else
                {
                    PrettyPrintCompileError(emitResult, logger);

                    return null!;
                }
            }
            finally
            {
                alc.Unload();
                // Finally do cleanup and release memory
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private static List<SyntaxTree> LoadSyntaxTree(string codeFolder)
        {
            var result = new List<SyntaxTree>(50);

            // Get the paths for all .cs files recursively in app folder
            IEnumerable<string>? csFiles = Directory.EnumerateFiles(codeFolder, "*.cs", SearchOption.AllDirectories);
            result.AddRange(from csFile in csFiles
                            let fs = new FileStream(csFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                            let sourceText = SourceText.From(fs, encoding: Encoding.UTF8, canBeEmbedded: true)
                            let syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText, path: csFile)
                            select syntaxTree);
            return result;
        }

        public static IEnumerable<MetadataReference> GetDefaultReferences()
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
                        MetadataReference.CreateFromFile(typeof(NetDaemonRxApp).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(RunnerService).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(NetDaemonHost).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(System.Reactive.Linq.Observable).Assembly.Location),
                    };

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
                    metaDataReference.Add(MetadataReference.CreateFromFile(assembly.Location));
            }

            metaDataReference.Add(MetadataReference.CreateFromFile((Assembly.GetEntryAssembly()?.Location)!));

            return metaDataReference;
        }

        private static CSharpCompilation GetCsCompilation(string codeFolder)
        {
            var syntaxTrees = LoadSyntaxTree(codeFolder);
            var metaDataReference = GetDefaultReferences();

            return CSharpCompilation.Create(
                $"net_{Path.GetRandomFileName()}.dll",
                syntaxTrees.ToArray(),
                references: metaDataReference.ToArray(),
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default
                )
            );
        }

        private static void PrettyPrintCompileError(EmitResult emitResult, ILogger logger)
        {
            var msg = new StringBuilder();
            msg.AppendLine("Compiler error!");

            foreach (var emitResultDiagnostic in emitResult.Diagnostics)
            {
                if (emitResultDiagnostic.Severity == DiagnosticSeverity.Error)
                {
                    msg.AppendLine(emitResultDiagnostic.ToString());
                }
            }

            logger.LogError(msg.ToString());
        }
        /// <summary>
        ///     All NetDaemonApp methods that needs to be closed with Execute or ExecuteAsync
        /// </summary>
        private static readonly string[] ExecuteWarningOnInvocationNames = new string[]
        {
            "Entity",
            "Entities",
            "Event",
            "Events",
            "InputSelect",
            "InputSelects",
            "MediaPlayer",
            "MediaPlayers",
            "Camera",
            "Cameras",
            "RunScript"
        };

        private static void InterceptAppInfo(SyntaxTree syntaxTree, CSharpCompilation compilation)
        {
            var semModel = compilation.GetSemanticModel(syntaxTree);

            foreach (var classDeclaration in syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var symbol = semModel?.GetDeclaredSymbol(classDeclaration);

                if (symbol is null)
                    continue;

                if (symbol.BaseType?.Name != "NetDaemonApp" && symbol.BaseType?.Name != "NetDaemonRxApp" && symbol.BaseType?.Name != "GeneratedAppBase")
                    continue;

                if (!classDeclaration.HasStructuredTrivia)
                    continue;

                foreach (var kind in classDeclaration.GetLeadingTrivia())
                {
                    switch (kind.Kind())
                    {
                        case SyntaxKind.SingleLineDocumentationCommentTrivia:
                            var doc = kind.ToString();
                            doc = System.Text.RegularExpressions.Regex.Replace(doc, @"(?i)s*<\s*(summary)\s*>", string.Empty);
                            doc = System.Text.RegularExpressions.Regex.Replace(doc, @"(?i)s*<\s*(\/summary)\s*>", string.Empty);
                            doc = System.Text.RegularExpressions.Regex.Replace(doc, @"(?i)\/\/\/", "");
                            var comment = new StringBuilder();

                            foreach (var row in doc.Split('\n'))
                            {
                                var commentRow = row.Trim();
                                if (commentRow.Length > 0)
                                    comment.AppendJoin(commentRow, Environment.NewLine);
                            }

                            var app_key = symbol.ContainingNamespace.Name?.Length == 0 ? symbol.Name : symbol.ContainingNamespace.Name + "." + symbol.Name;

                            if (!NetDaemonAppBase.CompileTimeProperties.ContainsKey(app_key))
                            {
                                NetDaemonAppBase.CompileTimeProperties[app_key] = new Dictionary<string, string>();
                            }

                            NetDaemonAppBase.CompileTimeProperties[app_key]["description"] = comment.ToString();

                            break;
                    }
                }
            }
        }
        /// <summary>
        ///     Warn user if fluent command chain not ending with Execute or ExecuteAsync
        /// </summary>
        /// <param name="syntaxTree">The parsed syntax tree</param>
        /// <param name="compilation">Compilated code</param>
        private static void WarnIfExecuteIsMissing(SyntaxTree syntaxTree, CSharpCompilation compilation, ILogger logger)
        {
            var semModel = compilation.GetSemanticModel(syntaxTree);

            var invocationExpressions = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>();
            var linesReported = new List<int>();

            foreach (var invocationExpression in invocationExpressions)
            {
                var symbol = (IMethodSymbol?)semModel?.GetSymbolInfo(invocationExpression).Symbol;
                if (symbol is null)
                    continue;

                if (string.IsNullOrEmpty(symbol.Name) ||
                    !ExecuteWarningOnInvocationNames.Contains(symbol.Name))
                {
                    // The invocation name is empty or not in list of invocations
                    // that needs to be closed with Execute or ExecuteAsync
                    continue;
                }

                // Now find top invocation to match whole expression
                InvocationExpressionSyntax topInvocationExpression = invocationExpression;

                if (symbol.ContainingType.Name == "NetDaemonApp")
                {
                    var disableLogging = false;

                    var symbolName = symbol.Name;

                    SyntaxNode? parentInvocationExpression = invocationExpression.Parent;

                    while (parentInvocationExpression is not null)
                    {
                        if (parentInvocationExpression is MethodDeclarationSyntax methodDeclarationSyntax && ExpressionContainsDisableLogging(methodDeclarationSyntax))
                        {
                            disableLogging = true;
                        }
                        if (parentInvocationExpression is InvocationExpressionSyntax invocationExpressionSyntax)
                        {
                            var parentSymbol = (IMethodSymbol?)semModel?.GetSymbolInfo(invocationExpression).Symbol;
                            if (parentSymbol?.Name == symbolName)
                                topInvocationExpression = invocationExpressionSyntax;
                        }
                        parentInvocationExpression = parentInvocationExpression.Parent;
                    }

                    // Now when we have the top InvocationExpression,
                    // lets check for Execute and ExecuteAsync
                    if (!ExpressionContainsExecuteInvocations(topInvocationExpression) && !disableLogging)
                    {
                        var x = syntaxTree.GetLineSpan(topInvocationExpression.Span);
                        if (!linesReported.Contains(x.StartLinePosition.Line))
                        {
                            // var startLinePosition = x.StartLinePosition.Line + 1;
                            logger.LogError($"Missing Execute or ExecuteAsync in {syntaxTree.FilePath} ({x.StartLinePosition.Line + 1},{x.StartLinePosition.Character + 1}) near {topInvocationExpression.ToFullString().Trim()}");
                            linesReported.Add(x.StartLinePosition.Line);
                        }
                    }
                }
            }
        }

        private static bool ExpressionContainsDisableLogging(MethodDeclarationSyntax methodInvocationExpression)
        {
            var invocationString = methodInvocationExpression.ToFullString();
            return invocationString.Contains("[DisableLog", StringComparison.InvariantCultureIgnoreCase)
                   && invocationString.Contains("SupressLogType.MissingExecute", StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool ExpressionContainsExecuteInvocations(InvocationExpressionSyntax invocation)
        {
            var invocationString = invocation.ToFullString();

            return invocationString.Contains("ExecuteAsync()", StringComparison.InvariantCultureIgnoreCase)
                   || invocationString.Contains("Execute()", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}