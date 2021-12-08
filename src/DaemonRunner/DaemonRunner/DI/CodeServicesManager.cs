using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetDaemon.Common.Exceptions;
using NetDaemon.Daemon;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.DI
{
    public sealed class CodeServicesManager : IInstanceDaemonAppServiceConfigurator
    {
        private readonly ILogger<CodeServicesManager> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="daemonAppServicesTypes">App services compiled app types</param>
        /// <param name="daemonAppTypes">App compiled app types</param>
        /// <param name="logger">ILogger instance to use</param>
        public CodeServicesManager(IEnumerable<Type> daemonAppServicesTypes, IEnumerable<Type> daemonAppTypes, ILogger<CodeServicesManager> logger)
        {
            _logger = logger;
            DaemonAppServicesTypes = daemonAppServicesTypes;
            DaemonAppTypes = daemonAppTypes;
        }

        [SuppressMessage("", "CA1065")]
        public int Count => DaemonAppServicesTypes.Count();

        // Internal for testing
        internal IEnumerable<Type> DaemonAppServicesTypes { get; }
        internal IEnumerable<Type> DaemonAppTypes { get; }

        
        public IServiceCollection ConfigureServices(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            // Instantiate all service configuration classes
            var servicesInstances = InstantiateServicesTypes(serviceProvider);

            // Add each app as a service
            foreach (var appType in DaemonAppTypes)
            {
                services.AddSingleton(appType, provider =>
                {
                    var host = provider.GetRequiredService<NetDaemonHost>();
                    
                    // rework get apps from host
                    var app = host.GetNetApp(appType.Name);
                    
                    if (app?.GetType() == appType)
                    {
                        return app;
                    }

                    throw new NetDaemonException($"App with class {appType.FullName} not initialized");
                });
            }
            
            // Add daemon app services
            foreach (object appServices in servicesInstances)
            {
                Type serviceType = appServices.GetType();
                try
                {
                    var configFn = appServices.GetType().GetMethod("ConfigureServices", BindingFlags.Public | BindingFlags.Instance);
                
                    if (configFn != null)
                    {
                        configFn.Invoke(appServices, new object?[]{services});
                        _logger.LogInformation("Successfully loaded app services {AppServicesId} ({Class})", serviceType.Name, serviceType.FullName);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error loading app services {AppServicesId} ({Class})", serviceType.Name, serviceType.FullName);
                    throw;
                }
                
            }
            
            return services;
        }

        private IEnumerable<object> InstantiateServicesTypes(ServiceProvider serviceProvider)
        {
            foreach (Type appServicesType in DaemonAppServicesTypes)
            {
                yield return ActivatorUtilities.CreateInstance(serviceProvider, appServicesType);
            }
        }
    }
}