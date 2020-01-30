using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;

namespace JoySoftware.HomeAssistant.NetDaemon.DaemonRunner
{
    public class CSGlobals
    {
        public INetDaemon GlobalDaemon;

    }
    public class CSScriptManager
    {
        private ScriptOptions _scriptOptions;
        private readonly string _codeFolder;
        private readonly ILogger _logger;
        private readonly INetDaemon _daemon;
        private CSGlobals _globals;
        private CSScriptManager() { }

        public CSScriptManager(string codeFolder, INetDaemon daemon, ILoggerFactory loggerFactory)
        {
            _codeFolder = codeFolder;
            _logger = loggerFactory.CreateLogger<CSScriptManager>();
            _daemon = daemon;
            _globals = new CSGlobals { GlobalDaemon = daemon };

            _scriptOptions = LoadAssembliesAndStandardImports();
        }

        public async Task LoadSources()
        {
            //var script = CSharpScript.Create("// Dummy code", _scriptOptions, globalsType: typeof(CSGlobals));

            foreach (var file in Directory.EnumerateFiles(_codeFolder, "*.cs", SearchOption.AllDirectories))
            {
                _logger.LogDebug($"Found cs file {Path.GetFileName(file)}");

                var script = CSharpScript.Create(File.ReadAllText(file), _scriptOptions, globalsType: typeof(CSGlobals));

                var compilation = script.GetCompilation();

                var stream = new MemoryStream();
                var emitResult = compilation.Emit(stream);

                if (emitResult.Success)
                {
                    var asm = Assembly.Load(stream.ToArray());

                    var apps = asm.GetTypes().Where(type => type.IsClass && type.IsSubclassOf(typeof(NetDaemonApp)));
                    foreach (var app in apps)
                    {
                        _logger.LogInformation($"Loading App ({app.Name})");
                        var daempnApp = (NetDaemonApp)Activator.CreateInstance(app);
                        await daempnApp.StartUpAsync(_daemon);
                        await daempnApp.InitializeAsync();
                    }
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
                if (assembly.FullName.StartsWith("NetDaemon"))
                    options = options.AddReferences(assembly);
            }

            // Add the standard imports
            options = options.AddImports("System");
            options = options.AddImports("JoySoftware.HomeAssistant.NetDaemon.Common");
            return options;
        }
    }
}
