using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NetDaemon.Common
{
    /// <summary>
    ///     Interface that all NetDaemon apps needs to implement
    /// </summary>
    public interface INetDaemon : INetDaemonCommon
    {
        /// <summary>
        ///     Returns true if NetDaemon is connected to Home Assistant
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        ///     Http features of NetDaemon is exposed through the Http property
        /// </summary>
        IHttpHandler Http { get; }

        /// <summary>
        ///     Calls a service
        /// </summary>
        /// <param name="domain">The domain of the service</param>
        /// <param name="service">The service being called</param>
        /// <param name="data">Any data that the service requires</param>
        /// <param name="waitForResponse">Waits for Home Assistant to return result before returning</param>
        void CallService(string domain, string service, dynamic? data = null, bool waitForResponse = false);

        /// <summary>
        ///     Calls a service
        /// </summary>
        /// <param name="domain">The domain of the service</param>
        /// <param name="service">The service being called</param>
        /// <param name="data">Any data that the service requires</param>
        /// <param name="waitForResponse">If we should wait for the service to get response from Home Assistant or send/forget scenario</param>
        Task CallServiceAsync(string domain, string service, dynamic? data = null, bool waitForResponse = false);

        /// <summary>
        ///     Trigger a state change using trigger templates
        /// </summary>
        /// <param name="id">webhook id</param>
        /// <param name="data">data being sent</param>
        /// <param name="waitForResponse">If we should wait for the service to get response from Home Assistant or send/forget scenario</param>
        void TriggerWebhook(string id, object? data, bool waitForResponse = false);

        /// <summary>
        ///     Get application instance by application instance id
        /// </summary>
        /// <param name="appInstanceId">The identity of the app instance</param>
        INetDaemonAppBase? GetApp(string appInstanceId);

        /// <summary>
        ///     Set entity state
        /// </summary>
        /// <param name="entityId">Entity unique id</param>
        /// <param name="state">The state that being set, only primitives are supported</param>
        /// <param name="attributes">Attributes, use anonomous types and lowercase letters</param>
        /// <param name="waitForResponse">If true, waits for object to set and returns the new EntityState else returns null</param>
        /// <returns>Returns entitystate if waitForResponse is true</returns>
        EntityState? SetState(string entityId, dynamic state, dynamic? attributes = null, bool waitForResponse = false);

        /// <summary>
        ///     Access the underlying service provider for IOT access to services
        /// </summary>
        public IServiceProvider? ServiceProvider { get; }
    }

    /// <summary>
    ///     Shared features in both Reactive and async/await models
    /// </summary>
    [SuppressMessage("", "CA1716")]
    public interface INetDaemonAppBase :
        INetDaemonInitialableApp, INetDaemonAppLogging, IAsyncDisposable, IEquatable<INetDaemonAppBase>
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

    /// <summary>
    ///     Interface for logging capabilities in NetDaemon Apps
    /// </summary>
    public interface INetDaemonAppLogging
    {
        /// <summary>
        ///     Logs an informational message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void Log(Exception exception, string message, params object[] param);

        /// <summary>
        ///     Logs an informational message
        /// </summary>
        /// <param name="message">The message to log</param>
        void LogInformation(string message);

        /// <summary>
        ///     Logs an informational message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        void LogInformation(Exception exception, string message);

        /// <summary>
        ///     Logs an informational message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogInformation(string message, params object[] param);

        /// <summary>
        ///     Logs an informational message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogInformation(Exception exception, string message, params object[] param);

        /// <summary>
        ///     Logs a debug message
        /// </summary>
        /// <param name="message">The message to log</param>
        void LogDebug(string message);

        /// <summary>
        ///     Logs a debug message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        void LogDebug(Exception exception, string message);

        /// <summary>
        ///     Logs a debug message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogDebug(string message, params object[] param);

        /// <summary>
        ///     Logs a debug message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogDebug(Exception exception, string message, params object[] param);

        /// <summary>
        ///     Logs an error message
        /// </summary>
        /// <param name="message">The message to log</param>
        void LogError(string message);

        /// <summary>
        ///     Logs an error message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        void LogError(Exception exception, string message);

        /// <summary>
        ///     Logs an error message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogError(string message, params object[] param);

        /// <summary>
        ///     Logs an error message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogError(Exception exception, string message, params object[] param);

        /// <summary>
        ///     Logs a trace message
        /// </summary>
        /// <param name="message">The message to log</param>
        void LogTrace(string message);

        /// <summary>
        ///     Logs a trace message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        void LogTrace(Exception exception, string message);

        /// <summary>
        ///     Logs a trace message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogTrace(string message, params object[] param);

        /// <summary>
        ///     Logs a trace message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogTrace(Exception exception, string message, params object[] param);

        /// <summary>
        ///     Logs a warning message
        /// </summary>
        /// <param name="message">The message to log</param>
        void LogWarning(string message);

        /// <summary>
        ///     Logs a warning message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        void LogWarning(Exception exception, string message);

        /// <summary>
        ///     Logs a warning message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogWarning(string message, params object[] param);

        /// <summary>
        ///     Logs a warning message
        /// </summary>
        /// <param name="exception">A exception</param>
        /// <param name="message">The message to log</param>
        /// <param name="param">Params</param>
        void LogWarning(Exception exception, string message, params object[] param);
    }

    /// <summary>
    ///     The interface that interacts with the daemon main logic
    /// </summary>
    public interface INetDaemonCommon
    {
        /// <summary>
        ///     Logger to use
        /// </summary>
        ILogger? Logger { get; }

        /// <summary>
        ///     All current states for all known entities
        /// </summary>
        /// <remarks>
        ///     All states are read and cached at startup. Every state change updates the
        ///     states. There can be a small risk that the state is not updated
        ///     exactly when it happens but it should be fine. The SetState function
        ///     updates the state before sending.
        /// </remarks>
        [SuppressMessage("", "CA1721")]
        IEnumerable<EntityState> State { get; }

        /// <summary>
        ///     Loads persistent data from unique id
        /// </summary>
        /// <param name="id">Unique Id of the data</param>
        /// <returns>The data persistent or null if not exists</returns>
        Task<T?> GetDataAsync<T>(string id) where T : class;

        /// <summary>
        ///     Gets current state for the entity
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        /// <returns></returns>
        EntityState? GetState(string entityId);

        /// <summary>
        ///     Saves any data with unique id, data have to be json serializable
        /// </summary>
        /// <param name="id">Unique id for all apps</param>
        /// <param name="data">Dynamic data being saved</param>
        Task SaveDataAsync<T>(string id, T data);

        /// <summary>
        ///     Sends a custom event
        /// </summary>
        /// <param name="eventId">Any identity of the event</param>
        /// <param name="data">Any data sent with the event</param>
        Task<bool> SendEvent(string eventId, dynamic? data = null);

        /// <summary>
        ///     Set entity state
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        /// <param name="state">The state being set</param>
        /// <param name="attributes">Name/Value pair of the attribute</param>
        Task<EntityState?> SetStateAsync(string entityId, dynamic state, params (string name, object val)[] attributes);

        /// <summary>
        ///     Use text-to-speech to speak a message
        /// </summary>
        /// <param name="entityId">Unique id of the media player the speech should play</param>
        /// <param name="message">The message that will be spoken</param>
        void Speak(string entityId, string message);
    }

    /// <summary>
    ///     Apps that can be initialized
    /// </summary>
    public interface INetDaemonInitialableApp
    {
        /// <summary>
        /// Init the application sync, is called by the NetDaemon after startup
        /// </summary>
        void Initialize();

        /// <summary>
        /// Init the application async, is called by the NetDaemon after startup
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        ///     Restores the app state
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Restores the state of the storage object.!--
        /// </para>
        /// <para>    It is implemented async so state will be lazy saved</para>
        /// </remarks>
        Task RestoreAppStateAsync();

        /// <summary>
        ///     Saves the app state
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Saves the state of the storage object.!--
        /// </para>
        /// <para>    It is implemented async so state will be lazy saved</para>
        /// </remarks>
        void SaveAppState();

        /// <summary>
        /// Start the application, normally implemented by the base class
        /// </summary>
        /// <param name="daemon"></param>
        Task StartUpAsync(INetDaemon daemon);
    }
}