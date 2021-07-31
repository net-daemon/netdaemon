using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetDaemon.Common;
using NetDaemon.Common.Configuration;
using NetDaemon.Common.Exceptions;
using NetDaemon.Infrastructure.Extensions;

namespace NetDaemon.Service.App
{
    public class DaemonAppCompiler : IDaemonAppCompiler
    {
        private readonly ILogger<DaemonAppCompiler> _logger;

        private readonly string? _sourceFolder;
        public DaemonAppCompiler(ILogger<DaemonAppCompiler> logger, IOptions<NetDaemonSettings> netDaemonSettings)
        {
            _ = netDaemonSettings ??
               throw new NetDaemonArgumentNullException(nameof(netDaemonSettings));
            _logger = logger;
            NetDaemonSettings = netDaemonSettings;
            _sourceFolder = netDaemonSettings.Value.GetAppSourceDirectory();
        }

        public IOptions<NetDaemonSettings> NetDaemonSettings { get; }

        public IEnumerable<Type> GetApps()
        {
            _logger.LogDebug("Loading dynamically compiled apps...");
            var assembly = Load();
            var apps = assembly.GetTypesWhereSubclassOf<INetDaemonApp>();

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