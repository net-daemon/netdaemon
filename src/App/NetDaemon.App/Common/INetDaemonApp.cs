using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetDaemon.Common
{
    /// <summary>
    /// Interface to implement by any app to be hosted
    /// </summary>
    public interface INetDaemonApp
    {
        /// <summary>
        /// Init the application async, is called by the NetDaemon after startup
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        ///     The dependencies that needs to be initialized before this app
        /// </summary>
        IEnumerable<string> Dependencies => Array.Empty<string>();

        /// <summary>
        ///     Unique id of the application
        /// </summary>
        string? Id { get; set; }

        /// <summary>
        ///     Returns the description, is the decorating comment of app class
        /// </summary>
        string Description { get; }

        /// <summary>
        ///     Gets or sets a flag indicating whether this app is enabled.
        ///     This property can be controlled from Home Assistant.
        /// </summary>
        /// <remarks>
        ///     A disabled app will not be initialized during the discovery.
        /// </remarks>
        bool IsEnabled { get; set; }

        /// <summary>
        ///     Returns different runtime information about an app
        /// </summary>
        AppRuntimeInfo RuntimeInfo { get; }

        /// <summary>
        ///     Unique id of the application entity
        /// </summary>
        string EntityId => $"switch.netdaemon_{Id?.ToSafeHomeAssistantEntityId()}";
    }
}