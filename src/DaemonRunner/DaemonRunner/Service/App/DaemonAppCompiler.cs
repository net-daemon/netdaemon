using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetDaemon.Common;
using NetDaemon.Common.Configuration;
using NetDaemon.Infrastructure.Extensions;

namespace NetDaemon.Service.App
{
    public class DaemonAppCompiler : IDaemonAppCompiler
    {
        private readonly ILogger<DaemonAppCompiler> _logger;
        private readonly IOptions<NetDaemonSettings> _netDaemonSettings;

        public DaemonAppCompiler(ILogger<DaemonAppCompiler> logger, IOptions<NetDaemonSettings> netDaemonSettings)
        {
            _logger = logger;
            _netDaemonSettings = netDaemonSettings;
        }

        public IEnumerable<Type> GetApps()
        {
            var assembly = Load();
            var apps = assembly.GetTypesWhereSubclassOf<NetDaemonAppBase>();

            if (!apps.Any())
                _logger.LogWarning("No .cs files found, please add files to {sourceFolder}/apps", _netDaemonSettings.Value.SourceFolder);

            return apps;
        }

        public Assembly Load()
        {
            CollectibleAssemblyLoadContext alc;
            var appFolder = Path.Combine(_netDaemonSettings.Value.SourceFolder!, "apps");
            return DaemonCompiler.GetCompiledAppAssembly(out alc, appFolder!, _logger);
        }
    }
}