using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace NetDaemon.Common
{
    /// <summary>
    /// Context for NetDaemon application
    /// </summary>
    public abstract class ApplicationContext : IAsyncDisposable, IDisposable, IApplicationMetadata
    {
        private readonly IServiceScope? _serviceScope;

        private Action _configProvider;

        public static ApplicationContext Create(Type applicationType, string id, IServiceProvider serviceProvider, INetDaemon netDaemon)
        {
            if (typeof(INetDaemonAppBase).IsAssignableFrom(applicationType))
            {
                return new AppBaseApplicationContext(applicationType, id, serviceProvider, netDaemon);
            }
            else
            {
                return new NonBaseApplicationContext(applicationType, id, serviceProvider, netDaemon);
            }
        }

        public virtual void SetConfigProvider(Action configProvider)
        {
            _configProvider = configProvider;
        }

        /// <summary>
        /// Creates a new ApplicationContext
        /// </summary>
        protected ApplicationContext(Type applicationType, string id, IServiceProvider serviceProvider, INetDaemon netDaemon)
        {
            if (applicationType == null) throw new ArgumentNullException(nameof(applicationType));
            if (netDaemon == null) throw new ArgumentNullException(nameof(netDaemon));

            // Create a new ServiceScope for all objects we create for this app
            // this makes sure they will all be disposed along with the app
            _serviceScope = serviceProvider.CreateScope();
            ServiceProvider = _serviceScope.ServiceProvider;
            ApplicationType = applicationType;
            Id = id;
        }

        protected void ApplyConfig() => _configProvider();

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
        /// Intended for internal use only
        /// </summary>
        public static ApplicationContext CreateFromAppInstance(INetDaemonAppBase applicationInstance, INetDaemon netDaemonHost, IServiceProvider serviceProvider)
        {
            if (applicationInstance == null) throw new ArgumentNullException(nameof(applicationInstance));

            var applicationContext = new AppBaseApplicationContext(applicationInstance, applicationInstance.Id!, serviceProvider, netDaemonHost);

            return applicationContext;
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
        //
        // /// <summary>
        // ///     Returns the description, is the decorating comment of app class
        // /// </summary>
        // public string Description => ApplicationMetadata.Description
        //                               ?? ApplicationType.GetCustomAttribute<DescriptionAttribute>()
        //                                   ?.Description
        //                               ?? "";
        //
        // /// <summary>
        // ///     Gets or sets a flag indicating whether this app is enabled.
        // ///     This property can be controlled from Home Assistant.
        // /// </summary>
        // /// <remarks>
        // ///     A disabled app will not be initialized during the discovery.
        // /// </remarks>
        // public bool IsEnabled
        // {
        //     get => ApplicationMetadata.IsEnabled;
        //     set => ApplicationMetadata.IsEnabled = value;
        // }
        //
        // /// <summary>
        // ///     Returns different runtime information about an app
        // /// </summary>
        // public AppRuntimeInfo RuntimeInfo => ApplicationMetadata.RuntimeInfo;
        //
        // /// <summary>
        // ///     Unique id of the application entity
        // /// </summary>
        public string EntityId => $"switch.netdaemon_{Id?.ToSafeHomeAssistantEntityId()}";
        /// <summary>
        ///     Unique id of the application
        /// </summary>
        public string? Id { get; set; }
        public virtual string? Description { get; set; }
        public virtual bool IsEnabled { get; set; } = true;
        public virtual AppRuntimeInfo RuntimeInfo { get; } = new ();

        /// <summary>
        /// The CLR Type of the application
        /// </summary>
        public virtual Type ApplicationType { get;  }
        
        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            Dispose();

            if (ApplicationInstance is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            }

//            if (_persistenceService != null) await _persistenceService.DisposeAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            IsEnabled = false;
            (ApplicationInstance as IDisposable) ?.Dispose();
            _serviceScope?.Dispose();
        }
    }
}