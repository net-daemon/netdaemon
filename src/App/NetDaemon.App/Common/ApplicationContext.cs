using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace NetDaemon.Common
{
    internal class ApplicationMetaData : IApplicationMetadata
    {
        public string? Id { get; set; }
        public string? Description { get; set; } = null;
        public bool IsEnabled { get; set; } = true;
        public AppRuntimeInfo RuntimeInfo { get; } = new AppRuntimeInfo();
    }

    public class ApplicationContext
    {
        private readonly ILogger _logger;
        private IApplicationMetadata _applicationMetadata;

        public ApplicationContext(object applicationInstance, ILogger logger)
        {
            // For old style applications the metadata properties are stored in the app itself
            // we need to keep that for backwards compatibility, for new apps we will use a separate class for the
            // metadata properties
            _applicationMetadata = applicationInstance is IApplicationMetadata appAsMetaData
                ? appAsMetaData
                : new ApplicationMetaData();
          
            ApplicationInstance = applicationInstance;
            _logger = logger;
        }
        public ApplicationContext(IAsyncInitializable netDaemonApp) : this(netDaemonApp, NullLogger.Instance)
        { }
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


        public async Task UnloadAsync()
        {
            try
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
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to unload apps, {app_id}", Id);
            }
        }
    }
}