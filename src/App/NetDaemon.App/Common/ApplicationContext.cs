using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.Daemon.Services;

namespace NetDaemon.Common
{
    /// <summary>
    /// Context for NetDaemon application
    /// </summary>
    public class ApplicationContext : IAsyncDisposable
    {
        private readonly ILogger _logger;
        private IApplicationMetadata _applicationMetadata;
        private INetDaemonPersistantApp _persistantApp;

        /// <summary>
        /// Creates a new ApplicationContext
        /// </summary>
        public ApplicationContext(object applicationInstance, INetDaemon netDaemon, ILogger logger)
        {
            ApplicationInstance = applicationInstance;
            _logger = logger;

            if (applicationInstance is NetDaemonAppBase appBase)
            {
                // For applications based on NetDaemonAppBase the services are provided by the application itself
                // we need to keep that for backwards compatibility
                _applicationMetadata = appBase;
                _persistantApp = appBase;
            }
            else
            {
                _applicationMetadata = new ApplicationMetadata();
                _persistantApp = new ApplicationPersistenceService(_applicationMetadata, netDaemon, logger);
            }
        }

        /// <summary>
        /// Gets the reference to the Application Instance
        /// </summary>
        public object ApplicationInstance { get; }
        
        /// <summary>
        ///     Unique id of the application
        /// </summary>
        public string? Id
        {
            get => _applicationMetadata.Id;
            init => _applicationMetadata.Id = value;
        }

        /// <summary>
        ///     The dependencies that needs to be initialized before this app
        /// </summary>
        public IEnumerable<string> Dependencies { get; set; } = Array.Empty<string>();

        /// <summary>
        ///     Returns the description, is the decorating comment of app class
        /// </summary>
        public string? Description => _applicationMetadata.Description
                                      ?? ApplicationInstance.GetType().GetCustomAttribute<DescriptionAttribute>()?.Description 
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
            if (ApplicationInstance is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            }

            if (ApplicationInstance is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}