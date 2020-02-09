using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;

namespace JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service
{
    class CollectibleAssemblyLoadContext : AssemblyLoadContext
    {
        public CollectibleAssemblyLoadContext() : base(isCollectible: true)
        {
        }

        protected override Assembly Load(AssemblyName name)
        {
            return null;
        }
    }

    public class CSScriptManager
    {

        #region -- private fields

        private ScriptOptions _scriptOptions;
        private readonly string _codeFolder;
        private readonly ILogger _logger;
        private readonly INetDaemon _daemon;

        #endregion

        private CSScriptManager() { }

        public CSScriptManager(string codeFolder, INetDaemon daemon, ILoggerFactory loggerFactory)
        {
            _codeFolder = codeFolder;
            _logger = loggerFactory.CreateLogger<CSScriptManager>();
            _daemon = daemon;
            _scriptOptions = LoadAssembliesAndStandardImports();
        }

        public async Task LoadSources(IDaemonAppConfig _daemonAppConfig)
        {
            if (string.IsNullOrEmpty(_codeFolder))
            {
                _logger.LogError("Code folder config can not be NULL!");
                return;
            }

            var appTypes = new List<Type>(10);

            // First see if there are local apps (when developing and debugging)
            var apps = Assembly.GetEntryAssembly()?.GetTypes().Where(type => type.IsClass && type.IsSubclassOf(typeof(NetDaemonApp)));
            if (apps != null)
                foreach (var localAppType in apps)
                {
                    appTypes.Add(localAppType);
                }

            if (appTypes.Count == 0)
            {
                // Code folder is configured and we are not debugging with local apps
                foreach (var file in Directory.EnumerateFiles(_codeFolder, "*.cs", SearchOption.AllDirectories))
                {

                    _logger.LogDebug($"Found cs file {Path.GetFileName(file)}");

                    var script = CSharpScript.Create(File.ReadAllText(file), _scriptOptions);

                    var compilation = script.GetCompilation();

                    var stream = new MemoryStream();
                    var emitResult = compilation.Emit(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    if (emitResult.Success)
                    {
                        var alc = new CollectibleAssemblyLoadContext();
                        var asm = alc.LoadFromStream(stream);
                        var asseblyAppTypes = asm.GetTypes().Where(type => type.IsClass && type.IsSubclassOf(typeof(NetDaemonApp)));
                        foreach (var app in asseblyAppTypes)
                        {
                            appTypes.Add(app);
                        }
                        alc.Unload();
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                    }
                    else
                    {
                        var msg = new StringBuilder();
                        msg.AppendLine($"Compiler error in file: {file.Substring(_codeFolder.Length + 1)}");

                        foreach (var emitResultDiagnostic in emitResult.Diagnostics)
                        {
                            if (emitResultDiagnostic.Severity == DiagnosticSeverity.Error)
                            {
                                msg.AppendLine(emitResultDiagnostic.ToString());
                            }
                        }

                        _logger.LogWarning(msg.ToString());

                    }
                }


            }

            await _daemonAppConfig.InstanceFromDaemonAppConfigs(appTypes, _codeFolder);

            // The scripting consumes allot of memory so lets clean up now
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private ScriptOptions LoadAssembliesAndStandardImports()
        {
            // Load all assemblies from the current running daemon
            var options = ScriptOptions.Default;

            var assembliesFromCurrentAppDomain = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assembliesFromCurrentAppDomain)
            {
                // _logger.LogInformation(assembly.FullName);
                if (assembly.FullName != null && assembly.FullName.StartsWith("NetDaemon"))
                    options = options.AddReferences(assembly);
            }

            // Add the standard imports
            options = options.AddImports("System");
            options = options.AddImports("JoySoftware.HomeAssistant.NetDaemon.Common");
            return options;
        }
    }
}
