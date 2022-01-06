using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace NetDaemon.Common
{
    /// <summary>
    /// Context for NetDaemon application
    /// </summary>
    public abstract class ApplicationContext : IAsyncDisposable, IApplicationMetadata, IApplicationContext
    {
        private readonly IServiceScope? _serviceScope;
        private bool _disposed;

        private Action? _configProvider;

        /// <summary>
        /// Creates a concrete ApplicationContext based on the type of the application
        /// </summary>
        public static ApplicationContext Create(Type applicationType, string id, IServiceProvider serviceProvider)
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
        public static ApplicationContext CreateFromAppInstanceForTest(INetDaemonAppBase applicationInstance, IServiceProvider serviceProvider)
        {
            if (applicationInstance == null) throw new ArgumentNullException(nameof(applicationInstance));

            var applicationContext = new AppBaseApplicationContext(applicationInstance, serviceProvider);

            return applicationContext;
        }

        /// <summary>
        /// Registers a delagate that will apply the config to this application
        /// </summary>
        /// <param name="configProvider"></param>
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

        /// <summary>
        /// Applies the configuration of the app
        /// </summary>
        protected void ApplyConfig() => _configProvider?.Invoke();

        /// <summary>
        /// Restores the persisted state of the app after a reload
        /// </summary>
        public abstract Task RestoreStateAsync();

        /// <summary>
        /// Starts the app
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Calls InitializeAsync() and / or Initialize() on app that implement it
        /// </summary>
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

        /// <summary>
        /// List of apps this app depends on
        /// </summary>
        public IEnumerable<string> Dependencies { get; set; } = Array.Empty<string>();

        /// <inheritdoc/>
        public string EntityId => $"switch.netdaemon_{Id?.ToSafeHomeAssistantEntityId()}";

        /// <inheritdoc/>
        public string? Id { get; init; }

        /// <inheritdoc/>
        public virtual string? Description { get; set; }

        /// <inheritdoc/>
        [SuppressMessage("", "CA2119", Justification = "class should actually be internal")]
        public virtual bool IsEnabled { get; set; } = true;

        /// <summary>
        ///     Returns different runtime information about an app
        /// </summary>
        [SuppressMessage("", "CA2119", Justification = "class should actually be internal")]
        public virtual AppRuntimeInfo RuntimeInfo { get; } = new();

        /// <summary>
        /// The CLR Type of the application
        /// </summary>
        [SuppressMessage("", "CA2119", Justification = "class should actually be internal")]
        public virtual Type ApplicationType { get; }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            
            _disposed = true;
            
            if (ApplicationInstance is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            }

            else if (ApplicationInstance is IDisposable disposable)
            {
                disposable.Dispose();
            }

            if (_serviceScope is IAsyncDisposable asyncDisposable1)
            {
                await asyncDisposable1.DisposeAsync().ConfigureAwait(false);
            }

            GC.SuppressFinalize(this);
        }
    }
}