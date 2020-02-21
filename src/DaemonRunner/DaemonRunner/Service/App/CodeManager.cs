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
using JoySoftware.HomeAssistant.NetDaemon.Common;
using JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]
namespace JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.App
{
    class CollectibleAssemblyLoadContext : AssemblyLoadContext
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

                    await method.InvokeAsync(netDaemonApp, entityId, to!, from!);
                }
                catch (Exception e)
                {
                    _daemon.Logger.LogError(e, "Failed to invoke the ServiceCall funcition");
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
                        await method.InvokeAsync(netDaemonApp, expObject!);
                    }
                    catch (Exception e)
                    {
                        _daemon.Logger.LogError(e, "Failed to invoke the ServiceCall function");
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

    public class CodeManager
    {
        // private readonly ScriptOptions _scriptOptions;
        private readonly string _codeFolder;

        private readonly IList<Type> _loadedDaemonApps;
        private readonly YamlConfig _yamlConfig;

        public CodeManager(string codeFolder)
        {
            _codeFolder = codeFolder;
            _loadedDaemonApps = new List<Type>(100);

            _yamlConfig = new YamlConfig(codeFolder);

            LoadLocalAssemblyApplicationsForDevelopment();

            // If provided code folder and we dont have local loaded daemon apps
            if (!string.IsNullOrEmpty(_codeFolder) && _loadedDaemonApps.Count() == 0)
                LoadAllCodeToLoadContext();
        }

        public IEnumerable<Type> DaemonAppTypes => _loadedDaemonApps;

        public IEnumerable<INetDaemonApp> InstanceAndInitApplications(INetDaemon? host)
        {
            _ = (host as INetDaemon) ?? throw new ArgumentNullException(nameof(host));

            var result = new List<INetDaemonApp>();
            foreach (string file in _yamlConfig.GetAllConfigFilePaths())
            {
                var yamlAppConfig = new YamlAppConfig(DaemonAppTypes, File.OpenText(file), _yamlConfig, file);

                foreach (var appInstance in yamlAppConfig.Instances)
                {
                    result.Add(appInstance);
                    appInstance.StartUpAsync(host!);
                    appInstance.InitializeAsync();
                    host!.Logger.LogInformation($"Successfully loaded app {appInstance.GetType().Name}");
                }

            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            return result;
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

        private void LoadAllCodeToLoadContext()
        {
            var syntaxTrees = new List<SyntaxTree>();
            var alc = new CollectibleAssemblyLoadContext();

            using (var peStream = new MemoryStream())
            {
                foreach (var csFile in GetCsFiles(_codeFolder))
                {
                    var sourceText = SourceText.From(File.ReadAllText(csFile));
                    var syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText);
                    syntaxTrees.Add(syntaxTree);
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
                        optimizationLevel: OptimizationLevel.Release,
                        assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));


                var emitResult = compilation.Emit(peStream);
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
                    System.Console.WriteLine(err);
                }
            }
            alc.Unload();

            GC.Collect();
            GC.WaitForPendingFinalizers();

        }

        public static IEnumerable<string> GetCsFiles(string configFixturePath)
        {
            return Directory.EnumerateFiles(configFixturePath, "*.cs", SearchOption.AllDirectories);
        }

    }
}
