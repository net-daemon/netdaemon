using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace NetDaemon.Common
{
    /// <summary>
    ///     Shared features in both Reactive and async/await models
    /// </summary>
    [SuppressMessage("", "CA1716")]
    public interface INetDaemonAppBase : INetDaemonInitialableApp, IAsyncInitializable, IInitializable, INeatDaemonPersistantApp, INetDaemonAppLogging, IAsyncDisposable
    {
        /// <summary>
        ///     The dependencies that needs to be initialized before this app
        /// </summary>
        IEnumerable<string> Dependencies { get; set; }
        
        /// <summary>
        ///     A thread safe key/value dictionary to safely share states within and between apps in memory
        /// </summary>
        ConcurrentDictionary<string, object> Global { get; }

        /// <summary>
        ///     Http features of NetDaemon is exposed through the Http property
        /// </summary>
        IHttpHandler Http { get; }

        /// <summary>
        ///     Unique id of the application
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        ///     Unique id of the application entity
        /// </summary>
        public string EntityId { get; }

        /// <summary>
        ///     Returns the description, is the decorating comment of app class
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gets or sets a flag indicating whether this app is enabled.
        ///     This property can be controlled from Home Assistant.
        /// </summary>
        /// <remarks>
        ///     A disabled app will not be initialized during the discovery.
        /// </remarks>
        public bool IsEnabled { get; set; }

        /// <summary>
        ///     Access stateful data
        /// </summary>
        /// <remarks>
        ///     The dynamic setter will automatically persist the whole storage object
        /// </remarks>
        dynamic Storage { get; }

        /// <summary>
        ///     Get application instance by application instance id
        /// </summary>
        /// <param name="appInstanceId">The identity of the app instance</param>
        INetDaemonAppBase? GetApp(string appInstanceId);

        /// <summary>
        ///     Use text-to-speech to speak a message
        /// </summary>
        /// <param name="entityId">Unique id of the media player the speech should play</param>
        /// <param name="message">The message that will be spoken</param>
        void Speak(string entityId, string message);

        /// <summary>
        ///     Sets application instance attribute in Home Assistant
        /// </summary>
        /// <param name="attribute">Attribute name</param>
        /// <param name="value">Value to set, null removes the attribute</param>
        void SetAttribute(string attribute, object? value);

        /// <summary>
        ///     Listen to service calls
        /// </summary>
        /// <param name="domain">The domain of the service call</param>
        /// <param name="service">The service being called</param>
        /// <param name="action">The action to perform when service is called</param>
        void ListenServiceCall(string domain, string service,
            Func<dynamic?, Task> action);

        /// <summary>
        ///     Returns different runtime information about an app
        /// </summary>
        public AppRuntimeInfo RuntimeInfo { get; }

        /// <summary>
        ///     Returns all entities (EntityId) that are currently registered
        /// </summary>
        public IEnumerable<string> EntityIds { get; }

        /// <summary>
        ///     Returns the underlying ServiceProvider to use IOC instancing
        /// </summary>
        public IServiceProvider? ServiceProvider { get; }
    }
}