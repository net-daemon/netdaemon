using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;
using NetDaemon.Infrastructure.Extensions;

namespace NetDaemon.Service.App
{
    public class LocalDaemonAssemblyCompiler : IDaemonAssemblyCompiler
    {
        private readonly ILogger<DaemonAssemblyCompiler> _logger;

        public LocalDaemonAssemblyCompiler(ILogger<DaemonAssemblyCompiler> logger)
        {
            _logger = logger;
        }

        public IEnumerable<Assembly> Load()
        {
            _logger.LogDebug("Loading local app assemblies...");
            
            var binFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)!;
            var netDaemonDlls = Directory.GetFiles(binFolder, "NetDaemon.*.dll");

            var alreadyLoadedAssemblies = AssemblyLoadContext.Default.Assemblies
                .Where(x => !x.IsDynamic)
                .Select(x => x.Location)
                .ToList();

            foreach (var netDaemonDllToLoadDynamically in netDaemonDlls.Except(alreadyLoadedAssemblies))
            {
                _logger.LogTrace("Loading {Dll} into AssemblyLoadContext", netDaemonDllToLoadDynamically);
                AssemblyLoadContext.Default.LoadFromAssemblyPath(netDaemonDllToLoadDynamically);
            }
            
            _logger.LogDebug("Loaded app assemblies");

            return AssemblyLoadContext.Default.Assemblies;
        }
    }
}