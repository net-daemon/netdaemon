using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using NetDaemon.Infrastructure.Extensions;

namespace NetDaemon.Service.App
{
    public class DaemonAppServicesCompiler : IDaemonAppServicesCompiler
    {
        private readonly ILogger<DaemonAppServicesCompiler> _logger;

        public DaemonAppServicesCompiler(ILogger<DaemonAppServicesCompiler> logger)
        {
            _logger = logger;
        }
       

        public IEnumerable<Type> GetAppServices(IEnumerable<Assembly> assemblies)
        {
            _logger.LogDebug("Loading assembly app services...");

            var appServices = assemblies.SelectMany(x => x.GetAppServicesClasses()).ToList();

            if (appServices.Count == 0)
                _logger.LogWarning("No daemon app services found.");
            else
                _logger.LogDebug("Found total of {NumberOfApps} app services", appServices.Count);

            return appServices;
        }
    }
}