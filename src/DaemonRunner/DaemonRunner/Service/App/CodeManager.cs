using JoySoftware.HomeAssistant.NetDaemon.Common;
using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.Config;
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
using System.Threading.Tasks;

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

    internal sealed class DaemonCompiler
    {
        public static (IEnumerable<Type>, CollectibleAssemblyLoadContext?) GetDaemonApps(string codeFolder, ILogger logger)
        {
            var loadedApps = new List<Type>(50);

            // Load the internal apps (mainly for )
            var disableLoadLocalAssemblies = Environment.GetEnvironmentVariable("HASS_DISABLE_LOCAL_ASM");
            if (disableLoadLocalAssemblies is object && disableLoadLocalAssemblies == "true")
            {
                var localApps = LoadLocalAssemblyApplicationsForDevelopment();
                if (localApps is object)
                    loadedApps.AddRange(localApps);
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
                        MetadataReference.CreateFromFile(typeof(Microsoft.Extensions.Logging.Abstractions.NullLogger).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(JoySoftware.HomeAssistant.NetDaemon.Common.NetDaemonApp).Assembly.Location),
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

            metaDataReference.Add(MetadataReference.CreateFromFile(Assembly.GetEntryAssembly()?.Location));

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
                // alc.Unload();
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

    public sealed class CodeManager : IAsyncDisposable
    {
        private readonly string _codeFolder;
        private readonly ILogger _logger;
        private IEnumerable<Type>? _loadedDaemonApps;

        private readonly YamlConfig _yamlConfig;

        public CodeManager(string codeFolder, IEnumerable<Type> loadedDaemonApps, ILogger logger)
        {
            _codeFolder = codeFolder;
            _logger = logger;

            _logger.LogInformation("Loading code and configuration from {path}", Path.GetFullPath(codeFolder));

            _yamlConfig = new YamlConfig(codeFolder);
            _loadedDaemonApps = loadedDaemonApps;


        }



        public IEnumerable<Type>? DaemonAppTypes => _loadedDaemonApps;

        public async Task EnableApplicationDiscoveryServiceAsync(INetDaemonHost host, bool discoverServicesOnStartup)
        {
            host.ListenCompanionServiceCall("reload_apps", async (_) => await ReloadApplicationsAsync(host));

            RegisterAppSwitchesAndTheirStates(host);

            if (discoverServicesOnStartup)
            {
                await InstanceAndInitApplications(host).ConfigureAwait(false);
            }
        }

        private void RegisterAppSwitchesAndTheirStates(INetDaemonHost host)
        {
            host.ListenServiceCall("switch", "turn_on", async (data) =>
            {
                await SetStateOnDaemonAppSwitch("on", data).ConfigureAwait(false);
            });

            host.ListenServiceCall("switch", "turn_off", async (data) =>
            {
                await SetStateOnDaemonAppSwitch("off", data).ConfigureAwait(false);
            });

            host.ListenServiceCall("switch", "toggle", async (data) =>
            {
                try
                {
                    string? entityId = data?.entity_id;
                    if (entityId is null)
                        return;

                    var currentState = host.GetState(entityId)?.State as string;

                    if (currentState == "on")
                        await SetStateOnDaemonAppSwitch("off", data).ConfigureAwait(false);
                    else
                        await SetStateOnDaemonAppSwitch("on", data).ConfigureAwait(false);
                }
                catch (System.Exception e)
                {
                    _logger.LogWarning(e, "Failed to set state from netdaemon switch");
                }
            });

            async Task SetStateOnDaemonAppSwitch(string state, dynamic? data)
            {
                string? entityId = data?.entity_id;
                if (entityId is null)
                    return;

                if (!entityId.StartsWith("switch.netdaemon_"))
                    return; // We only want app switches

                List<(string, object)>? attributes = null;

                var entityAttributes = host.GetState(entityId)?.Attribute as IDictionary<string, object>;

                if (entityAttributes is object)
                    attributes = entityAttributes.Keys.Select(n => (n, entityAttributes[n])).ToList();

                if (attributes is object)
                    await host.SetStateAsync(entityId, state, attributes.ToArray()).ConfigureAwait(false);
                else
                    await host.SetStateAsync(entityId, state).ConfigureAwait(false);
            }
        }

        private async Task ReloadApplicationsAsync(INetDaemonHost host)
        {
            try
            {
                _loadedDaemonApps = null;

                await host.StopDaemonActivitiesAsync();

                await EnableApplicationDiscoveryServiceAsync(host, true).ConfigureAwait(false);
            }
            catch (System.Exception e)
            {
                host.Logger.LogError(e, "Failed to reload applications", e);
            }
        }

        public async Task<IEnumerable<INetDaemonAppBase>> InstanceAndInitApplications(INetDaemonHost? host)
        {
            _ = (host as INetDaemonHost) ?? throw new ArgumentNullException(nameof(host));

            await host!.UnloadAllApps().ConfigureAwait(false);

            var result = new List<INetDaemonAppBase>();

            if (DaemonAppTypes is null)
                return result;

            var allConfigFilePaths = _yamlConfig.GetAllConfigFilePaths();

            if (allConfigFilePaths.Count() == 0)
            {
                _logger.LogWarning("No yaml configuration files found, please add files to [netdaemonfolder]/apps");
                return result;
            }

            foreach (string file in allConfigFilePaths)
            {
                var yamlAppConfig = new YamlAppConfig(DaemonAppTypes, File.OpenText(file), _yamlConfig, file);

                foreach (var appInstance in yamlAppConfig.Instances)
                {
                    await appInstance.StartUpAsync(host!).ConfigureAwait(false);
                    await appInstance.RestoreAppStateAsync().ConfigureAwait(false);

                    if (!appInstance.IsEnabled)
                    {
                        await appInstance.DisposeAsync().ConfigureAwait(false);
                        host!.Logger.LogInformation("Skipped disabled app class {class}", appInstance.GetType().Name);
                        continue;
                    }

                    result.Add(appInstance);
                    // Register the instance with the host
                    host.RegisterAppInstance(appInstance.Id!, appInstance);
                }
            }
            if (result.SelectMany(n => n.Dependencies).Count() > 0)
            {
                // There are dependecies defined
                var edges = new HashSet<Tuple<INetDaemonAppBase, INetDaemonAppBase>>();

                foreach (var instance in result)
                {
                    foreach (var dependency in instance.Dependencies)
                    {
                        var dependentApp = result.Where(n => n.Id == dependency).FirstOrDefault();
                        if (dependentApp == null)
                            throw new ApplicationException($"There is no app named {dependency}, please check dependencies or make sure you have not disabled the dependent app!");

                        edges.Add(new Tuple<INetDaemonAppBase, INetDaemonAppBase>(instance, dependentApp));
                    }
                }
                var sortedInstances = TopologicalSort<INetDaemonAppBase>(result.ToHashSet(), edges) ??
                    throw new ApplicationException("Application dependencies is wrong, please check dependencies for circular dependencies!");

                result = sortedInstances;
            }

            foreach (var app in result)
            {
                // Init by calling the InitializeAsync
                var taskInitAsync = app.InitializeAsync();
                var taskAwaitedAsyncTask = await Task.WhenAny(taskInitAsync, Task.Delay(5000)).ConfigureAwait(false);
                if (taskAwaitedAsyncTask != taskInitAsync)
                    _logger.LogWarning("InitializeAsync of application {app} took longer that 5 seconds, make sure InitializeAsync is not blocking!", app.Id);

                // Init by calling the Initialize
                var taskInit = Task.Run(app.Initialize);
                var taskAwaitedTask = await Task.WhenAny(taskInit, Task.Delay(5000)).ConfigureAwait(false);
                if (taskAwaitedTask != taskInit)
                    _logger.LogWarning("Initialize of application {app} took longer that 5 seconds, make sure Initialize function is not blocking!", app.Id);

                await app.HandleAttributeInitialization(host!);
                host!.Logger.LogInformation("Successfully loaded app {appId} ({class})", app.Id, app.GetType().Name);
            }

            await host!.SetDaemonStateAsync(DaemonAppTypes?.Count() ?? 0, host.RunningAppInstances.Count()).ConfigureAwait(false);

            GC.Collect();
            GC.WaitForPendingFinalizers();

            return result;
        }

        /// <summary>
        /// Topological Sorting (Kahn's algorithm)
        /// </summary>
        /// <remarks>https://en.wikipedia.org/wiki/Topological_sorting</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="nodes">All nodes of directed acyclic graph.</param>
        /// <param name="edges">All edges of directed acyclic graph.</param>
        /// <returns>Sorted node in topological order.</returns>
        private static List<T>? TopologicalSort<T>(HashSet<T> nodes, HashSet<Tuple<T, T>> edges) where T : IEquatable<T>
        {
            // Empty list that will contain the sorted elements
            var L = new List<T>();

            // Set of all nodes with no incoming edges
            var S = new HashSet<T>(nodes.Where(n => edges.All(e => e.Item2.Equals(n) == false)));

            // while S is non-empty do
            while (S.Any())
            {
                //  remove a node n from S
                var n = S.First();
                S.Remove(n);

                // add n to tail of L
                L.Add(n);

                // for each node m with an edge e from n to m do
                foreach (var e in edges.Where(e => e.Item1.Equals(n)).ToList())
                {
                    var m = e.Item2;

                    // remove edge e from the graph
                    edges.Remove(e);

                    // if m has no other incoming edges then
                    if (edges.All(me => me.Item2.Equals(m) == false))
                    {
                        // insert m into S
                        S.Add(m);
                    }
                }
            }

            // if graph has edges then
            if (edges.Any())
            {
                // return error (graph has at least one cycle)
                return null;
            }
            else
            {
                L.Reverse();
                // return L (a topologically sorted order)
                return L;
            }
        }

        public void UnLoadCompilationFromGC()
        {
            _loadedDaemonApps = null;
        }
        public ValueTask DisposeAsync()
        {
            UnLoadCompilationFromGC();
            return new ValueTask();
        }
    }
}