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
using System.Dynamic;
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

    public static class DaemonAppExtensions
    {
        public static void HandleAttributeInitialization(this INetDaemonApp netDaemonApp, INetDaemon _daemon)
        {
            var netDaemonAppType = netDaemonApp.GetType();
            foreach (var method in netDaemonAppType.GetMethods())
            {
                foreach (var attr in method.GetCustomAttributes(false))
                {
                    switch (attr)
                    {
                        case HomeAssistantServiceCallAttribute hasstServiceCallAttribute:
                            HandleServiceCallAttribute(_daemon, netDaemonApp, method);
                            break;

                        case HomeAssistantStateChangedAttribute hassStateChangedAttribute:
                            HandleStateChangedAttribute(_daemon, hassStateChangedAttribute, netDaemonApp, method);
                            break;
                    }
                }
            }
        }

        private static void HandleStateChangedAttribute(
            INetDaemon _daemon,
            HomeAssistantStateChangedAttribute hassStateChangedAttribute,
            INetDaemonApp netDaemonApp,
            MethodInfo method
            )
        {
            var (signatureOk, err) = CheckIfStateChangedSignatureIsOk(method);

            if (!signatureOk)
            {
                _daemon.Logger.LogWarning(err);
                return;
            }

            _daemon.ListenState(hassStateChangedAttribute.EntityId,
            async (entityId, to, from) =>
            {
                try
                {
                    if (hassStateChangedAttribute.To != null)
                        if ((dynamic)hassStateChangedAttribute.To != to?.State)
                            return;

                    if (hassStateChangedAttribute.From != null)
                        if ((dynamic)hassStateChangedAttribute.From != from?.State)
                            return;

                    // If we don´t accept all changes in the state change
                    // and we do not have a state change so return
                    if (to?.State == from?.State && !hassStateChangedAttribute.AllChanges)
                        return;

                    await method.InvokeAsync(netDaemonApp, entityId, to!, from!).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _daemon.Logger.LogError(e, "Failed to invoke the ServiceCall function for app {appId}", netDaemonApp.Id);
                }
            });
        }

        private static void HandleServiceCallAttribute(INetDaemon _daemon, INetDaemonApp netDaemonApp, MethodInfo method)
        {
            var (signatureOk, err) = CheckIfServiceCallSignatureIsOk(method);
            if (!signatureOk)
            {
                _daemon.Logger.LogWarning(err);
                return;
            }

            dynamic serviceData = new FluentExpandoObject();
            serviceData.method = method.Name;
            serviceData.@class = netDaemonApp.GetType().Name;
            _daemon.CallService("netdaemon", "register_service", serviceData);

            _daemon.ListenServiceCall("netdaemon", $"{serviceData.@class}_{serviceData.method}",
                async (data) =>
                {
                    try
                    {
                        var expObject = data as ExpandoObject;
                        await method.InvokeAsync(netDaemonApp, expObject!).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        _daemon.Logger.LogError(e, "Failed to invoke the ServiceCall function for app {appId}", netDaemonApp);
                    }
                });
        }

        private static (bool, string) CheckIfServiceCallSignatureIsOk(MethodInfo method)
        {
            if (method.ReturnType != typeof(Task))
                return (false, $"{method.Name} has not correct return type, expected Task");

            var parameters = method.GetParameters();

            if (parameters == null || (parameters != null && parameters.Length != 1))
                return (false, $"{method.Name} has not correct number of parameters");

            var dynParam = parameters![0];
            if (dynParam.CustomAttributes.Count() == 1 &&
                dynParam.CustomAttributes.First().AttributeType == typeof(DynamicAttribute))
                return (true, string.Empty);

            return (false, $"{method.Name} is not correct signature");
        }

        private static (bool, string) CheckIfStateChangedSignatureIsOk(MethodInfo method)
        {
            if (method.ReturnType != typeof(Task))
                return (false, $"{method.Name} has not correct return type, expected Task");

            var parameters = method.GetParameters();

            if (parameters == null || (parameters != null && parameters.Length != 3))
                return (false, $"{method.Name} has not correct number of parameters");

            if (parameters![0].ParameterType != typeof(string))
                return (false, $"{method.Name} first parameter exepected to be string for entityId");

            if (parameters![1].ParameterType != typeof(EntityState))
                return (false, $"{method.Name} second parameter exepected to be EntityState for toState");

            if (parameters![2].ParameterType != typeof(EntityState))
                return (false, $"{method.Name} first parameter exepected to be EntityState for fromState");

            return (true, string.Empty);
        }
    }

    public sealed class CodeManager : IDisposable
    {
        private readonly string _codeFolder;
        private readonly ILogger _logger;
        private readonly List<Type> _loadedDaemonApps;

        private readonly YamlConfig _yamlConfig;

        private readonly List<INetDaemonApp> _instanciatedDaemonApps;

        public CodeManager(string codeFolder, ILogger logger)
        {
            _codeFolder = codeFolder;
            _logger = logger;
            _loadedDaemonApps = new List<Type>(100);
            _instanciatedDaemonApps = new List<INetDaemonApp>(100);

            _logger.LogInformation("Loading code and configuration from {path}", Path.GetFullPath(codeFolder));

            _yamlConfig = new YamlConfig(codeFolder);

            LoadLocalAssemblyApplicationsForDevelopment();
            CompileScriptsInCodeFolder();
        }

        public void Dispose()
        {
            foreach (var app in _instanciatedDaemonApps)
            {
                app.Dispose();
            }
        }

        public IEnumerable<Type> DaemonAppTypes => _loadedDaemonApps;

        public async Task EnableApplicationDiscoveryServiceAsync(INetDaemonHost host, bool discoverServicesOnStartup)
        {
            host.ListenCompanionServiceCall("reload_apps", async (_) => await ReloadApplicationsAsync(host));

            RegisterAppSwitchesAndTheirStates(host);

            if (discoverServicesOnStartup)
            {
                await InstanceAndInitApplications(host);
            }
        }

        private void RegisterAppSwitchesAndTheirStates(INetDaemonHost host)
        {
            host.ListenServiceCall("switch", "turn_on", async (data) =>
            {
                await SetStateOnDaemonAppSwitch("on", data);
            });

            host.ListenServiceCall("switch", "turn_off", async (data) =>
            {
                await SetStateOnDaemonAppSwitch("off", data);
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
                        await SetStateOnDaemonAppSwitch("off", data);
                    else
                        await SetStateOnDaemonAppSwitch("on", data);

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
                    await host.SetState(entityId, state, attributes.ToArray());
                else
                    await host.SetState(entityId, state);
            }
        }

        private async Task ReloadApplicationsAsync(INetDaemonHost host)
        {
            try
            {
                await host.StopDaemonActivitiesAsync();

                foreach (var app in _instanciatedDaemonApps)
                {
                    app.Dispose();
                }
                _instanciatedDaemonApps.Clear();
                _loadedDaemonApps.Clear();

                CompileScriptsInCodeFolder();
                RegisterAppSwitchesAndTheirStates(host);
                await InstanceAndInitApplications(host);
            }
            catch (System.Exception e)
            {
                host.Logger.LogError("Failed to reload applications", e);
            }
        }

        public async Task<IEnumerable<INetDaemonApp>> InstanceAndInitApplications(INetDaemonHost? host)
        {
            _ = (host as INetDaemonHost) ?? throw new ArgumentNullException(nameof(host));

            host!.ClearAppInstances();
            CompileScriptsInCodeFolder();

            var result = new List<INetDaemonApp>();
            var allConfigFilePaths = _yamlConfig.GetAllConfigFilePaths();

            if (allConfigFilePaths.Count() == 0)
            {
                _logger.LogWarning("No yaml configuration files found, please add files to [netdaemonfolder]/apps");
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
                        appInstance.Dispose();
                        host!.Logger.LogInformation("Skipped disabled app class {class}", appInstance.GetType().Name);
                        continue;
                    }

                    result.Add(appInstance);
                    // Register the instance with the host
                    host.RegisterAppInstance(appInstance.Id!, (appInstance as NetDaemonApp)!);

                }
            }
            if (result.SelectMany(n => n.Dependencies).Count() > 0)
            {
                // There are dependecies defined
                var edges = new HashSet<Tuple<INetDaemonApp, INetDaemonApp>>();

                foreach (var instance in result)
                {
                    foreach (var dependency in instance.Dependencies)
                    {
                        var dependentApp = result.Where(n => n.Id == dependency).FirstOrDefault();
                        if (dependentApp == null)
                            throw new ApplicationException($"There is no app named {dependency}, please check dependencies or make sure you have not disabled the dependent app!");

                        edges.Add(new Tuple<INetDaemonApp, INetDaemonApp>(instance, dependentApp));
                    }
                }
                var sortedInstances = TopologicalSort<INetDaemonApp>(result.ToHashSet(), edges) ??
                    throw new ApplicationException("Application dependencies is wrong, please check dependencies for circular dependencies!");

                result = sortedInstances;
            }

            foreach (var app in result)
            {
                await app.InitializeAsync().ConfigureAwait(false);
                app.HandleAttributeInitialization(host!);
                host!.Logger.LogInformation("Successfully loaded app {appId} ({class})", app.Id, app.GetType().Name);
            }

            _instanciatedDaemonApps.AddRange(result);
            await host!.SetDaemonStateAsync(_loadedDaemonApps.Count, _instanciatedDaemonApps.Count);

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
        static List<T>? TopologicalSort<T>(HashSet<T> nodes, HashSet<Tuple<T, T>> edges) where T : IEquatable<T>
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

        private void LoadLocalAssemblyApplicationsForDevelopment()
        {
            // Get daemon apps in entry assembly (mainly for development)
            var apps = Assembly.GetEntryAssembly()?.GetTypes().Where(type => type.IsClass && type.IsSubclassOf(typeof(NetDaemonApp)));
            if (apps != null)
                foreach (var localAppType in apps)
                {
                    _loadedDaemonApps.Add(localAppType);
                }
        }

        internal void CompileScriptsInCodeFolder()
        {
            // If provided code folder and we dont have local loaded daemon apps
            if (!string.IsNullOrEmpty(_codeFolder) && _loadedDaemonApps.Count() == 0)
                LoadAllCodeToLoadContext();
        }

        private void LoadAllCodeToLoadContext()
        {
            var syntaxTrees = new List<SyntaxTree>();
            var alc = new CollectibleAssemblyLoadContext();

            using (var peStream = new MemoryStream())
            using (var symbolsStream = new MemoryStream())
            {
                var csFiles = GetCsFiles(_codeFolder);
                if (csFiles.Count() == 0 && _loadedDaemonApps.Count() == 0)
                {
                    // Only log when not have locally built assemblies, typically in dev environment
                    _logger.LogWarning("No .cs files files found, please add files to [netdaemonfolder]/apps");
                }
                var embeddedTexts = new List<EmbeddedText>();

                foreach (var csFile in csFiles)
                {
                    using (var fs = new FileStream(csFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var sourceText = SourceText.From(fs, encoding: Encoding.UTF8, canBeEmbedded: true);
                        embeddedTexts.Add(EmbeddedText.FromSource(csFile, sourceText));

                        var syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText, path: csFile);
                        syntaxTrees.Add(syntaxTree);
                    }
                }

                var metaDataReference = new List<MetadataReference>(10)
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Microsoft.Extensions.Logging.Abstractions.NullLogger).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
                };

                var assembliesFromCurrentAppDomain = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assembliesFromCurrentAppDomain)
                {
                    if (assembly.FullName != null
                        && !assembly.FullName.Contains("Dynamic")
                        && !string.IsNullOrEmpty(assembly.Location))
                        metaDataReference.Add(MetadataReference.CreateFromFile(assembly.Location));
                }

                var compilation = CSharpCompilation.Create("netdaemondynamic.dll",
                    syntaxTrees.ToArray(),
                    references: metaDataReference.ToArray(),
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                        optimizationLevel: OptimizationLevel.Debug,
                        assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));

                foreach (var syntaxTree in syntaxTrees)
                {
                    WarnIfExecuteIsMissing(syntaxTree, compilation);
                }

                var emitOptions = new EmitOptions(
                        debugInformationFormat: DebugInformationFormat.PortablePdb,
                        pdbFilePath: "netdaemondynamic.pdb");

                var emitResult = compilation.Emit(
                    peStream: peStream,
                    pdbStream: symbolsStream,
                    embeddedTexts: embeddedTexts,
                    options: emitOptions);

                if (emitResult.Success)
                {
                    peStream.Seek(0, SeekOrigin.Begin);

                    var asm = alc.LoadFromStream(peStream);
                    var assemblyAppTypes = asm.GetTypes().Where(type => type.IsClass && type.IsSubclassOf(typeof(NetDaemonApp)));
                    foreach (var app in assemblyAppTypes)
                    {
                        _loadedDaemonApps.Add(app);
                    }
                }
                else
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
                    var err = msg.ToString();
                    _logger.LogError(err);
                }
            }

            // Finally do cleanup and release memory
            alc.Unload();
            GC.Collect();
            GC.WaitForPendingFinalizers();
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
        private void WarnIfExecuteIsMissing(SyntaxTree syntaxTree, CSharpCompilation compilation)
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
                            _logger.LogError($"Missing Execute or ExecuteAsync in {syntaxTree.FilePath} ({x.StartLinePosition.Line + 1},{x.StartLinePosition.Character + 1}) near {topInvocationExpression.ToFullString().Trim()}");
                            linesReported.Add(x.StartLinePosition.Line);
                        }
                    }
                }
            }
        }

        // Todo: Refactor using something smarter than string match. In the future use Roslyn
        private bool ExpressionContainsDisableLogging(MethodDeclarationSyntax methodInvocationExpression)
        {
            var invocationString = methodInvocationExpression.ToFullString();
            if (invocationString.Contains("[DisableLog") && invocationString.Contains("SupressLogType.MissingExecute"))
            {
                return true;
            }
            return false;
        }

        // Todo: Refactor using something smarter than string match. In the future use Roslyn
        private bool ExpressionContainsExecuteInvocations(InvocationExpressionSyntax invocation)
        {
            var invocationString = invocation.ToFullString();

            if (invocationString.Contains("ExecuteAsync()") || invocationString.Contains("Execute()"))
            {
                return true;
            }

            return false;
        }

        public static IEnumerable<string> GetCsFiles(string configFixturePath)
        {
            return Directory.EnumerateFiles(configFixturePath, "*.cs", SearchOption.AllDirectories);
        }
    }
}