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

        private readonly string? _sourceFolder = null;
        public DaemonAppCompiler(ILogger<DaemonAppCompiler> logger, IOptions<NetDaemonSettings> netDaemonSettings)
        {
            _logger = logger;
            _netDaemonSettings = netDaemonSettings;
            _sourceFolder = netDaemonSettings.Value.GetAppSourceDirectory();

        }

        public IOptions<NetDaemonSettings> NetDaemonSettings => _netDaemonSettings;

        public IEnumerable<Type> GetApps()
        {
            _logger.LogDebug("Loading dynamically compiled apps...");
            var assembly = Load();
            var apps = assembly.GetTypesWhereSubclassOf<NetDaemonAppBase>();

            if (!apps.Any())
                _logger.LogWarning("No .cs files found, please add files to {sourceFolder}", _sourceFolder);
            else
                _logger.LogDebug("Found total of {nr_of_apps} apps", apps.Count());

            return apps;
        }

        public Assembly Load()
        {
            return DaemonCompiler.GetCompiledAppAssembly(out _, _sourceFolder!, _logger);
        }
    }
}