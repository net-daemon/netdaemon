﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace NetDaemon.Common
{
    /// <summary>
    /// Context for NetDaemon application
    /// </summary>
    public abstract class ApplicationContext : IAsyncDisposable, IApplicationMetadata
    {
        private readonly IServiceScope? _serviceScope;

        private Action? _configProvider;

        /// <summary>
        /// Creates a concrete ApplicationContext based on the type of the application
        /// </summary>
        public static ApplicationContext Create(Type applicationType, string id, IServiceProvider serviceProvider, INetDaemon netDaemon)
        {
            // Create a new ServiceScope for all objects we create for this app
            // this makes sure they will all be disposed along with the app
            if (applicationType == null) throw new ArgumentNullException(nameof(applicationType));

            if (typeof(INetDaemonAppBase).IsAssignableFrom(applicationType))
            {
                return new AppBaseApplicationContext(applicationType, id, serviceProvider);
            }
            else
            {
                return new NonBaseApplicationContext(applicationType, id, serviceProvider);
            }
        }
        
        /// <summary>
        /// Intended for internal use by fakes only
        /// </summary>
        public static ApplicationContext CreateFromAppInstance(INetDaemonAppBase applicationInstance, IServiceProvider serviceProvider)
        {
            if (applicationInstance == null) throw new ArgumentNullException(nameof(applicationInstance));

            var applicationContext = new AppBaseApplicationContext(applicationInstance, serviceProvider);

            return applicationContext;
        }

        public virtual void SetConfigProvider(Action configProvider)
        {
            _configProvider = configProvider;
        }

        /// <summary>
        /// Creates a new ApplicationContext
        /// </summary>
        protected ApplicationContext(Type applicationType, string id, IServiceProvider serviceProvider)
        {
            // Create a new ServiceScope for all objects we create for this app
            // this makes sure they will all be disposed along with the app
            _serviceScope = serviceProvider.CreateScope();
            ServiceProvider = _serviceScope.ServiceProvider;

            // make sure this ApplicationContext can be resolved  
            var appScope = ServiceProvider.GetService<ApplicationScope>();
            if (appScope != null)
            {
                appScope.ApplicationContext = this;
            }

            ApplicationType = applicationType ?? throw new ArgumentNullException(nameof(applicationType));
            Id = id;
        }

        protected void ApplyConfig() => _configProvider?.Invoke();

        public abstract Task RestoreStateAsync();

        public abstract void InstantiateApp();

        public async Task InitializeAsync()
        {
            if (ApplicationInstance is IAsyncInitializable asyncInitializable)
            {
                // Init by calling the InitializeAsync
                await asyncInitializable.InitializeAsync().ConfigureAwait(false);
            }

            if (ApplicationInstance is IInitializable initalizableApp)
            {
                // Init by calling the Initialize
                initalizableApp.Initialize();
            }
        }

        /// <summary>
        /// Gets the reference to the Application Instance
        /// </summary>
        public object? ApplicationInstance { get; protected set; }

        /// <summary>
        /// ServiceProvider scoped for this application
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        public IEnumerable<string> Dependencies { get; set; } = Array.Empty<string>();

        /// <inheritdoc/>
        public string EntityId => $"switch.netdaemon_{Id?.ToSafeHomeAssistantEntityId()}";

        /// <inheritdoc/>
        public string? Id { get; set; }

        /// <inheritdoc/>
        public virtual string? Description { get; set; }

        /// <inheritdoc/>
        public virtual bool IsEnabled { get; set; } = true;

        /// <summary>
        ///     Returns different runtime information about an app
        /// </summary>
        public virtual AppRuntimeInfo RuntimeInfo { get; } = new();

        /// <summary>
        /// The CLR Type of the application
        /// </summary>
        public virtual Type ApplicationType { get; }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (ApplicationInstance is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            }
            
            if (_serviceScope is IAsyncDisposable asyncDisposable1)
            {
                asyncDisposable1.DisposeAsync();
            }
            
        }
    }
}