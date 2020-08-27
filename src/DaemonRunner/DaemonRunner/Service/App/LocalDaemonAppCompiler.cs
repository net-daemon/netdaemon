using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;
using NetDaemon.Infrastructure.Extensions;

namespace NetDaemon.Service.App
{
    public class LocalDaemonAppCompiler : IDaemonAppCompiler
    {
        private readonly ILogger<DaemonAppCompiler> _logger;

        public LocalDaemonAppCompiler(ILogger<DaemonAppCompiler> logger)
        {
            _logger = logger;
        }

        public IEnumerable<Type> GetApps()
        {
            var assembly = Load();

            var apps = assembly.GetTypesWhereSubclassOf<NetDaemonAppBase>();

            if (!apps.Any())
                _logger.LogWarning("No local daemon apps found.");

            return apps;
        }

        public Assembly Load()
        {
            return Assembly.GetEntryAssembly()!;
        }
    }
}