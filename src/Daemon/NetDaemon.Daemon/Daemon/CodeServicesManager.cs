using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetDaemon.Daemon.Config;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.Daemon
{
    public sealed class CodeServicesManager : IInstanceDaemonAppServiceCollection
    {
        private readonly ILogger<CodeServicesManager> _logger;
        private readonly IYamlConfig _yamlConfig;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="daemonAppServicesTypes">App services compiled app types</param>
        /// <param name="daemonAppTypes">App compiled app types</param>
        /// <param name="logger">ILogger instance to use</param>
        /// <param name="yamlConfig"></param>
        public CodeServicesManager(IEnumerable<Type> daemonAppServicesTypes, IEnumerable<Type> daemonAppTypes, ILogger<CodeServicesManager> logger, IYamlConfig yamlConfig)
        {
            _logger = logger;
            DaemonAppServicesTypes = daemonAppServicesTypes;
            DaemonAppTypes = daemonAppTypes;
            _yamlConfig = yamlConfig;
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

                    throw new Exception("app not found");
                });
            }
            
            // Add daemon app services
            foreach (object appServices in servicesInstances)
            {
                var configFn = appServices.GetType().GetMethod("ConfigureServices", BindingFlags.Public | BindingFlags.Instance);
                
                if (configFn != null)
                {
                    configFn.Invoke(appServices, new object?[]{services});
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

        class AppsServiceProvider : IServiceProvider, IServiceScopeFactory, IDisposable
        {
            private readonly IServiceProvider _parentProvider;
            private readonly IServiceProvider _appsProvider;

            public AppsServiceProvider(IServiceProvider parentProvider, IServiceProvider appsProvider)
            {
                _parentProvider = parentProvider;
                _appsProvider = appsProvider;
            }
            
            public object? GetService(Type serviceType)
            {
                if (serviceType == typeof(IServiceScopeFactory))
                {
                    return this;
                }
                
                var appsService = _appsProvider.GetService(serviceType);

                if (appsService != null)
                {
                    return appsService;
                }
                
                var parentService = _parentProvider.GetService(serviceType);

                if (parentService != null)
                {
                    return parentService;
                }

                return null;
                
                // return _appsProvider.GetService(serviceType) ?? _parentProvider.GetService(serviceType);
            }
            
            public IServiceScope CreateScope()
            {
                var parentProviderScope = _parentProvider.CreateScope();
                var appsProviderScope = _appsProvider.CreateScope();
                
                return new AppsServiceProviderScope(parentProviderScope, appsProviderScope);
            }

            public void Dispose()
            {
                if (_appsProvider is IDisposable disposableAppsProvider)
                {
                    disposableAppsProvider.Dispose();
                }
            }
        }

        class AppsServiceProviderScope : IServiceScope
        {
            private readonly IServiceScope _parentProviderScope;
            private readonly IServiceScope _appsProviderScope;

            public AppsServiceProviderScope(IServiceScope parentProviderScope, IServiceScope appsProviderScope)
            {
                _parentProviderScope = parentProviderScope;
                _appsProviderScope = appsProviderScope;

                ServiceProvider = new AppsServiceProvider(_parentProviderScope.ServiceProvider, _appsProviderScope.ServiceProvider);
            }

            public void Dispose()
            {
                _parentProviderScope.Dispose();
                _appsProviderScope.Dispose();
            }

            public IServiceProvider ServiceProvider { get; }
        }
    }
}