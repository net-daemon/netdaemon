using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace NetDaemon.Common
{
    /// <summary>
    /// Context for NetDaemon application
    /// </summary>
    public sealed class ApplicationContext : IAsyncDisposable, IDisposable
    {
        private IApplicationMetadata _applicationMetadata;
        private IList<IDisposable> _toDispose = new List<IDisposable>();

        /// <summary>
        /// Creates a new ApplicationContext
        /// </summary>
        public ApplicationContext(object applicationInstance)
        {
            ApplicationInstance = applicationInstance;

            if (applicationInstance is NetDaemonAppBase appBase)
            {
                // For applications based on NetDaemonAppBase the services are provided by the application itself
                // we need to keep that for backwards compatibility
                _applicationMetadata = appBase;
            }
            else
            {
                _applicationMetadata = new ApplicationMetadata();
            }
        }

        public void TrackDisposable(IDisposable toDispose)
        {
            _toDispose.Add(toDispose);
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
            if (ApplicationInstance is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            }

            if (ApplicationInstance is IDisposable disposable)
            {
                disposable.Dispose();
            }

            foreach (var trackedDisposable in _toDispose)
            {
                trackedDisposable.Dispose();
            }
        }

        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }
    }
}