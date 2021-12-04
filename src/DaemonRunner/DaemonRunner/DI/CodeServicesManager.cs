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
using NetDaemon.Daemon.Config;

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