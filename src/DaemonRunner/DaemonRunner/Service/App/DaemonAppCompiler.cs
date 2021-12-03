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
        
        public DaemonAppCompiler(ILogger<DaemonAppCompiler> logger)
        {
            _logger = logger;
        }

        public IOptions<NetDaemonSettings> NetDaemonSettings { get; }

        

        public IEnumerable<Type> GetApps(IEnumerable<Assembly> assemblies)
        {
            _logger.LogDebug("Loading apps...");
            
            var apps = assemblies.SelectMany(x => x.GetAppClasses()).ToList();

            if (!apps.Any())
                _logger.LogWarning("No daemon apps found.");
            else
                _logger.LogDebug("Found total of {NumberOfApps} apps", apps.Count());

            return apps;
        }
    }
}