﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Daemon.Services;

namespace NetDaemon.Common
{
    /// <summary>
    /// Context for NetDaemon application
    /// </summary>
    public sealed class ApplicationContext : IAsyncDisposable, IDisposable
    {
        private readonly IServiceScope? _serviceScope;
        private readonly IApplicationMetadata _applicationMetadata;
        private readonly IPersistenceService _persistenceService;

        /// <summary>
        /// Creates a new ApplicationContext
        /// </summary>
        public ApplicationContext(Type applicationType, string id, IServiceProvider serviceProvider)
        {
            // Create a new ServiceScope for all objects we create for this app
            // this makes sure they will all be disposed along with the app
            _serviceScope = serviceProvider.CreateScope();
            ServiceProvider = _serviceScope.ServiceProvider;


            if (applicationType.IsAssignableFrom(typeof(NetDaemonAppBase)))
            {
                var app = (NetDaemonAppBase)ActivatorUtilities.GetServiceOrCreateInstance(ServiceProvider, applicationType);

                app.PersistenceService = _persistenceService;
                _applicationMetadata = app;
            }
            else
            {
                _applicationMetadata = new ApplicationMetadata(applicationType);
                _persistenceService = new ApplicationPersistenceService(_applicationMetadata, ServiceProvider.GetRequiredService<INetDaemon>());

                if (_applicationMetadata.IsEnabled)
                {
                    ApplicationInstance = ActivatorUtilities.GetServiceOrCreateInstance(ServiceProvider, applicationType);
                }
            }

            Id = id;
        }

        /// <summary>
        /// Creates a new ApplicationContext
        /// </summary>
        private ApplicationContext(INetDaemonAppBase applicationInstance, IServiceProvider serviceProvider)
        {
            ApplicationInstance = applicationInstance;
            ServiceProvider = serviceProvider;

            _applicationMetadata = GetInitializeMetaData();
            _persistenceService = GetPersistanceService();
        }

        /// <summary>
        /// Intended for internal use only
        /// </summary>
        public static ApplicationContext CreateFromAppInstance(INetDaemonAppBase applicationInstance, IServiceProvider serviceProvider)
        {
            return new ApplicationContext(applicationInstance, serviceProvider);
        }

        private IPersistenceService GetPersistanceService()
        {
            if (ApplicationInstance is IPersistenceService service)
            {
                return service;
            }

            return _persistenceService;
        }

        private IApplicationMetadata GetInitializeMetaData()
        {
            if (ApplicationInstance is IApplicationMetadata appBase)
            {
                // For applications based on NetDaemonAppBase the services are provided by the application itself
                // we need to keep that for backwards compatibility
                return appBase;
            }
            return new ApplicationMetadata(ApplicationInstance.GetType());
        }

        /// <summary>
        /// Gets the reference to the Application Instance
        /// </summary>
        public object ApplicationInstance { get; }

        /// <summary>
        /// ServiceProvider scoped for this application
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        ///     Unique id of the application
        /// </summary>
        public string? Id
        {
            get => _applicationMetadata.Id;
            private init => _applicationMetadata.Id = value;
        }

        /// <summary>
        ///     The dependencies that needs to be initialized before this app
        /// </summary>
        public IEnumerable<string> Dependencies { get; set; } = Array.Empty<string>();

        /// <summary>
        ///     Returns the description, is the decorating comment of app class
        /// </summary>
        public string Description => _applicationMetadata.Description
                                      ?? ApplicationInstance.GetType().GetCustomAttribute<DescriptionAttribute>()
                                          ?.Description
                                      ?? "";

        /// <summary>
        ///     Gets or sets a flag indicating whether this app is enabled.
        ///     This property can be controlled from Home Assistant.
        /// </summary>
        /// <remarks>
        ///     A disabled app will not be initialized during the discovery.
        /// </remarks>
        public bool IsEnabled
        {
            get => _applicationMetadata.IsEnabled;
            set => _applicationMetadata.IsEnabled = value;
        }

        /// <summary>
        ///     Returns different runtime information about an app
        /// </summary>
        public AppRuntimeInfo RuntimeInfo => _applicationMetadata.RuntimeInfo;

        /// <summary>
        ///     Unique id of the application entity
        /// </summary>
        public string EntityId => _applicationMetadata.EntityId;

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            Dispose();

            if (ApplicationInstance is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            IsEnabled = false;
            (ApplicationInstance as IDisposable) ?.Dispose();
            _serviceScope?.Dispose();
        }

        public void SaveAppState() => _persistenceService.SaveAppState();

        public Task RestoreAppStateAsync() => _persistenceService.RestoreAppStateAsync();
    }
}