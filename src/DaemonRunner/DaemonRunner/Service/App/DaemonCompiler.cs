using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using NetDaemon.Common;
using NetDaemon.Common.Reactive;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.App
{

    internal class CollectibleAssemblyLoadContext : AssemblyLoadContext
    {
        public CollectibleAssemblyLoadContext() : base(isCollectible: true)
        {
        }

        protected override Assembly? Load(AssemblyName _) => null;
    }

    /// <summary>
    ///     Compiles the code into a collectable context
    /// </summary>
    internal sealed class DaemonCompiler
    {
        public static (IEnumerable<Type>, CollectibleAssemblyLoadContext?) GetDaemonApps(string codeFolder, ILogger logger)
        {
            var loadedApps = new List<Type>(50);

            // Load the internal apps (mainly for )
            var disableLoadLocalAssemblies = Environment.GetEnvironmentVariable("HASS_DISABLE_LOCAL_ASM");
            if (!(disableLoadLocalAssemblies is object && disableLoadLocalAssemblies == "true"))
            {
                var localApps = LoadLocalAssemblyApplicationsForDevelopment();
                if (localApps is object)
                    loadedApps.AddRange(localApps);
            }
            if (loadedApps.Count() > 0)
            {
                // We do not want to get and compile the apps if it is includer
                // this is typically when in dev environment
                logger.LogInformation("Loading compiled built-in apps");
                return (loadedApps, null);
            }
            CollectibleAssemblyLoadContext alc;
            // Load the compiled apps
            var (compiledApps, compileErrorText) = GetCompiledApps(out alc, codeFolder, logger);

            if (compiledApps is object)
                loadedApps.AddRange(compiledApps);
            else if (string.IsNullOrEmpty(compileErrorText) == false)
                logger.LogError(compileErrorText);
            else if (loadedApps.Count == 0)
                logger.LogWarning("No .cs files files found, please add files to [netdaemonfolder]/apps");

            return (loadedApps, alc);
        }

        private static IEnumerable<Type>? LoadLocalAssemblyApplicationsForDevelopment()
        {
            // Get daemon apps in entry assembly (mainly for development)
            return Assembly.GetEntryAssembly()?.GetTypes()
                .Where(type => type.IsClass && type.IsSubclassOf(typeof(NetDaemonAppBase)));
        }

        private static List<SyntaxTree> LoadSyntaxTree(string codeFolder)
        {
            var result = new List<SyntaxTree>(50);

            // Get the paths for all .cs files recurcivlely in app folder
            var csFiles = Directory.EnumerateFiles(codeFolder, "*.cs", SearchOption.AllDirectories);


            var embeddedTexts = new List<EmbeddedText>();

            foreach (var csFile in csFiles)
            {
                using (var fs = new FileStream(csFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var sourceText = SourceText.From(fs, encoding: Encoding.UTF8, canBeEmbedded: true);
                    embeddedTexts.Add(EmbeddedText.FromSource(csFile, sourceText));

                    var syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText, path: csFile);
                    result.Add(syntaxTree);
                }
            }
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
                        MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.DynamicExpression).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Microsoft.Extensions.Logging.Abstractions.NullLogger).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(NetDaemonApp).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(NetDaemonRxApp).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.RunnerService).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(JoySoftware.HomeAssistant.NetDaemon.Daemon.NetDaemonHost).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(System.Reactive.Linq.Observable).Assembly.Location),
                    };

            var assembliesFromCurrentAppDomain = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assembliesFromCurrentAppDomain)
            {
                if (assembly.FullName != null
                    && !assembly.FullName.Contains("Dynamic")
                    && !string.IsNullOrEmpty(assembly.Location))
                    metaDataReference.Add(MetadataReference.CreateFromFile(assembly.Location));
            }

            metaDataReference.Add(MetadataReference.CreateFromFile(Assembly.GetEntryAssembly()?.Location!));

            return metaDataReference;
        }

        private static CSharpCompilation GetCsCompilation(string codeFolder)
        {
            var syntaxTrees = LoadSyntaxTree(codeFolder);
            var metaDataReference = GetDefaultReferences();


            return CSharpCompilation.Create($"net_{Path.GetRandomFileName()}.dll",
                syntaxTrees.ToArray(),
                references: metaDataReference.ToArray(),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
        }

        public static (IEnumerable<Type>?, string) GetCompiledApps(out CollectibleAssemblyLoadContext alc, string codeFolder, ILogger logger)
        {

            alc = new CollectibleAssemblyLoadContext();


            try
            {
                var compilation = GetCsCompilation(codeFolder);

                foreach (var syntaxTree in compilation.SyntaxTrees)
                {
                    if (Path.GetFileName(syntaxTree.FilePath) != "_EntityExtensions.cs")
                        WarnIfExecuteIsMissing(syntaxTree, compilation, logger);
                }

                // var emitOptions = new EmitOptions(
                //         debugInformationFormat: DebugInformationFormat.PortablePdb,
                //         pdbFilePath: "netdaemondynamic.pdb");

                using (var peStream = new MemoryStream())
                // using (var symbolsStream = new MemoryStream())
                {
                    var emitResult = compilation.Emit(
                        peStream: peStream
                        // pdbStream: symbolsStream,
                        // embeddedTexts: embeddedTexts,
                        /*options: emitOptions*/);

                    if (emitResult.Success)
                    {
                        peStream.Seek(0, SeekOrigin.Begin);

                        var asm = alc!.LoadFromStream(peStream);
                        return (asm.GetTypes() // Get all types
                                .Where(type => type.IsClass && type.IsSubclassOf(typeof(NetDaemonAppBase))) // That is a app
                                    , ""); // And return a list apps
                    }
                    else
                    {
                        return (null, PrettyPrintCompileError(emitResult));
                    }
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

        private static string PrettyPrintCompileError(EmitResult emitResult)
        {
            var msg = new StringBuilder();
            msg.AppendLine($"Compiler error!");

            foreach (var emitResultDiagnostic in emitResult.Diagnostics)
            {
                if (emitResultDiagnostic.Severity == DiagnosticSeverity.Error)
                {
                    msg.AppendLine(emitResultDiagnostic.ToString());
                }
            }
            return msg.ToString();

        }
        /// <summary>
        ///     All NetDaemonApp methods that needs to be closed with Execute or ExecuteAsync
        /// </summary>
        private static string[] _executeWarningOnInvocationNames = new string[]
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

                if (string.IsNullOrEmpty(symbol?.Name) ||
                    _executeWarningOnInvocationNames.Contains(symbol?.Name) == false)
                    // The invocation name is empty or not in list of invocations
                    // that needs to be closed with Execute or ExecuteAsync
                    continue;

                // Now find top invocation to match whole expression
                InvocationExpressionSyntax topInvocationExpression = invocationExpression;

                if (symbol is object && symbol.ContainingType.Name == "NetDaemonApp")
                {
                    var disableLogging = false;

                    var symbolName = symbol.Name;

                    SyntaxNode? parentInvocationExpression = invocationExpression.Parent;

                    while (parentInvocationExpression is object)
                    {
                        if (parentInvocationExpression is MethodDeclarationSyntax)
                        {
                            if (ExpressionContainsDisableLogging((MethodDeclarationSyntax)parentInvocationExpression))
                            {
                                disableLogging = true;
                            }
                        }
                        if (parentInvocationExpression is InvocationExpressionSyntax)
                        {
                            var parentSymbol = (IMethodSymbol?)semModel?.GetSymbolInfo(invocationExpression).Symbol;
                            if (parentSymbol?.Name == symbolName)
                                topInvocationExpression = (InvocationExpressionSyntax)parentInvocationExpression;
                        }
                        parentInvocationExpression = parentInvocationExpression.Parent;
                    }

                    // Now when we have the top InvocationExpression,
                    // lets check for Execute and ExecuteAsync
                    if (ExpressionContainsExecuteInvocations(topInvocationExpression) == false && disableLogging == false)
                    {
                        var x = syntaxTree.GetLineSpan(topInvocationExpression.Span);
                        if (linesReported.Contains(x.StartLinePosition.Line) == false)
                        {
                            logger.LogError($"Missing Execute or ExecuteAsync in {syntaxTree.FilePath} ({x.StartLinePosition.Line + 1},{x.StartLinePosition.Character + 1}) near {topInvocationExpression.ToFullString().Trim()}");
                            linesReported.Add(x.StartLinePosition.Line);
                        }
                    }
                }
            }
        }

        // Todo: Refactor using something smarter than string match. In the future use Roslyn
        private static bool ExpressionContainsDisableLogging(MethodDeclarationSyntax methodInvocationExpression)
        {
            var invocationString = methodInvocationExpression.ToFullString();
            if (invocationString.Contains("[DisableLog") && invocationString.Contains("SupressLogType.MissingExecute"))
            {
                return true;
            }
            return false;
        }

        // Todo: Refactor using something smarter than string match. In the future use Roslyn
        private static bool ExpressionContainsExecuteInvocations(InvocationExpressionSyntax invocation)
        {
            var invocationString = invocation.ToFullString();

            if (invocationString.Contains("ExecuteAsync()") || invocationString.Contains("Execute()"))
            {
                return true;
            }

            return false;
        }

    }
}