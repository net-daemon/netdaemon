using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.CSharp.Scripting;
// using Microsoft.CodeAnalysis.Emit;
// using Microsoft.CodeAnalysis.Scripting;
// using Microsoft.CodeAnalysis.CSharp;
// using Microsoft.CodeAnalysis.Text;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]
namespace JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.App
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
    public class CodeManager
    {
        // private readonly ScriptOptions _scriptOptions;
        private readonly string _codeFolder;

        private readonly IList<Type> _loadedDaemonApps;
        public CodeManager(string codeFolder)
        {
            _codeFolder = codeFolder;
            _loadedDaemonApps = new List<Type>(100);
            // _scriptOptions = LoadAssembliesAndStandardImports();

            if (!string.IsNullOrEmpty(_codeFolder))
                LoadAllCodeToLoadContext();
        }

        public IEnumerable<INetDaemonApp> InstanceAndInitApplications(INetDaemonHost host)
        {
            var result = new List<INetDaemonApp>();
            foreach (string file in Directory.EnumerateFiles(_codeFolder, "*.yaml", SearchOption.AllDirectories))
            {
                var appInstance = DaemonAppTypes.InstanceFromYamlConfig(File.OpenText(file));
                if (appInstance != null)
                    result.Add(appInstance);
            }            
            return result;
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
                    MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),

                };

                var assembliesFromCurrentAppDomain = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assembliesFromCurrentAppDomain)
                {
                    // _logger.LogInformation(assembly.FullName);
                    if (assembly.FullName != null && assembly.FullName.StartsWith("NetDaemon"))
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

        }

        public IEnumerable<Type> DaemonAppTypes => _loadedDaemonApps;

        public static IEnumerable<string> GetCsFiles(string configFixturePath)
        {
            return Directory.EnumerateFiles(configFixturePath, "*.cs", SearchOption.AllDirectories);
        }

    }
}



// private void LoadAllCodeToLoadContext2()
// {
//     var stream = new MemoryStream();
//     var alc = new CollectibleAssemblyLoadContext();

//     // EmitResult emitResult;
//     // Script<object>? script = null;
//     foreach (var csFile in GetCsFiles(_codeFolder))
//     {
//         var script = CSharpScript.Create(File.ReadAllText(csFile), _scriptOptions);
//         var compilation = script.GetCompilation();
//         var emitResult = compilation.Emit(stream);

//         stream.Seek(0, SeekOrigin.Begin);
//         if (emitResult.Success) //
//         {

//             var asm = alc.LoadFromStream(stream);
//             var assemblyAppTypes = asm.GetTypes().Where(type => type.IsClass && type.IsSubclassOf(typeof(NetDaemonApp)));
//             foreach (var app in assemblyAppTypes)
//             {
//                 _loadedDaemonApps.Add(app);
//             }


//         }
//         else
//         {
//             var msg = new StringBuilder();
//             msg.AppendLine($"Compiler error!");

//             foreach (var emitResultDiagnostic in emitResult.Diagnostics)
//             {
//                 if (emitResultDiagnostic.Severity == DiagnosticSeverity.Error)
//                 {
//                     msg.AppendLine(emitResultDiagnostic.ToString());
//                 }
//             }
//             var err = msg.ToString();
//             System.Console.WriteLine(err);

//         }
//     }
//     alc.Unload();
//     GC.Collect();
//     GC.WaitForPendingFinalizers();




// }

// private ScriptOptions LoadAssembliesAndStandardImports()
// {
//     // Load all assemblies from the current running daemon
//     var options = ScriptOptions.Default;

//     var assembliesFromCurrentAppDomain = AppDomain.CurrentDomain.GetAssemblies();
//     foreach (var assembly in assembliesFromCurrentAppDomain)
//     {
//         // _logger.LogInformation(assembly.FullName);
//         if (assembly.FullName != null && assembly.FullName.StartsWith("NetDaemon"))
//             options = options.AddReferences(assembly);
//     }

//     // Add the standard imports
//     options = options.AddImports("System");
//     options = options.AddImports("JoySoftware.HomeAssistant.NetDaemon.Common");
//     return options;
// }
