using JoySoftware.HomeAssistant.NetDaemon.Common.Reactive;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    /// <summary>
    ///     Interface that all NetDaemon apps needs to implement
    /// </summary>
    public interface INetDaemon : INetDaemonCommon
    {
        /// <summary>
        ///     The observable implementation for event changes
        /// </summary>
        IRxEvent EventChanges { get; }

        /// <summary>
        ///     Http features of NetDaemon is exposed through the Http property
        /// </summary>
        IHttpHandler Http { get; }

        /// <summary>
        ///     The observable implementation for state changes
        /// </summary>
        IRxStateChange StateChanges { get; }
        /// <summary>
        ///     Calls a service
        /// </summary>
        /// <param name="domain">The domain of the service</param>
        /// <param name="service">The service being called</param>
        /// <param name="data">Any data that the service requires</param>
        void CallService(string domain, string service, dynamic? data = null);

        /// <summary>
        ///     Selects one or more camera entities to do action on
        /// </summary>
        /// <param name="app">The Daemon App calling fluent API</param>
        /// <param name="entityIds">Entity unique id:s</param>
        ICamera Camera(INetDaemonApp app, params string[] entityIds);

        /// <summary>
        ///     Selects one or more cameras entities to do action on
        /// </summary>
        /// <param name="app">The Daemon App calling fluent API</param>
        /// <param name="entityIds">Entity unique id:s</param>
        ICamera Cameras(INetDaemonApp app, IEnumerable<string> entityIds);

        /// <summary>
        ///     Selects one or more cameras entities to do action on using lambda
        /// </summary>
        /// <param name="app">The Daemon App calling fluent API</param>
        /// <param name="func">The lambda expression selecting mediaplayers</param>
        ICamera Cameras(INetDaemonApp app, Func<IEntityProperties, bool> func);

        /// <summary>
        ///     Selects one or more entities to do action on
        /// </summary>
        /// <param name="app">The Daemon App calling fluent API</param>
        /// <param name="entityId">The unique id of the entity</param>
        IEntity Entities(INetDaemonApp app, IEnumerable<string> entityId);

        /// <summary>
        ///     Selects one or more entities to do action on using lambda
        /// </summary>
        /// <param name="app">The Daemon App calling fluent API</param>
        /// <param name="func">The lambda expression for selecting entities</param>
        IEntity Entities(INetDaemonApp app, Func<IEntityProperties, bool> func);

        /// <summary>
        ///     Selects one or more entities to do action on
        /// </summary>
        /// <param name="app">The Daemon App calling fluent API</param>
        /// <param name="entityId">The unique id of the entity</param>
        IEntity Entity(INetDaemonApp app, params string[] entityId);

        /// <summary>
        ///     Selects one or more input select to do action on
        /// </summary>
        /// <param name="app">The Daemon App calling fluent API</param>
        /// <param name="inputSelectParams">Events</param>
        IFluentInputSelect InputSelect(INetDaemonApp app, params string[] inputSelectParams);

        /// <summary>
        ///     Selects one or more input selects to do action on
        /// </summary>
        /// <param name="app">The Daemon App calling fluent API</param>
        /// <param name="inputSelectParams">Events</param>
        IFluentInputSelect InputSelects(INetDaemonApp app, IEnumerable<string> inputSelectParams);

        /// <summary>
        ///     Selects the input selects to do actions on using lambda
        /// </summary>
        /// <param name="func">The lambda expression selecting input select</param>
        /// <param name="app">The Daemon App calling fluent API</param>
        IFluentInputSelect InputSelects(INetDaemonApp app, Func<IEntityProperties, bool> func);

        /// <summary>
        ///     Listen to service calls
        /// </summary>
        /// <param name="domain">The domain of the service call</param>
        /// <param name="service">The service being called</param>
        /// <param name="action">The action to perform when service is called</param>
        void ListenServiceCall(string domain, string service,
            Func<dynamic?, Task> action);

        /// <summary>
        ///     Selects one or more media player entities to do action on
        /// </summary>
        /// <param name="app">The Daemon App calling fluent API</param>
        /// <param name="entityIds">Entity unique id:s</param>
        IMediaPlayer MediaPlayer(INetDaemonApp app, params string[] entityIds);

        /// <summary>
        ///     Selects one or more media player entities to do action on
        /// </summary>
        /// <param name="app">The Daemon App calling fluent API</param>
        /// <param name="entityIds">Entity unique id:s</param>
        IMediaPlayer MediaPlayers(INetDaemonApp app, IEnumerable<string> entityIds);

        /// <summary>
        ///     Selects one or more media player entities to do action on using lambda
        /// </summary>
        /// <param name="app">The Daemon App calling fluent API</param>
        /// <param name="func">The lambda expression selecting mediaplayers</param>
        IMediaPlayer MediaPlayers(INetDaemonApp app, Func<IEntityProperties, bool> func);

        /// <summary>
        ///     Runs one or more scripts
        /// </summary>
        /// <param name="app">The Daemon App calling fluent API</param>
        /// <param name="entityIds">The unique id:s of the script</param>
        IScript RunScript(INetDaemonApp app, params string[] entityIds);
        /// <summary>
        ///     Set entity state
        /// </summary>
        /// <param name="entityId">Entity unique id</param>
        /// <param name="state">The state that being set, only primitives are supported</param>
        /// <param name="attributes">Attributes, use anonomous types and lowercase letters</param>
        void SetState(string entityId, dynamic state, dynamic? attributes = null);
    }

    /// <summary>
    ///     Base interface that all NetDaemon apps needs to implement
    /// </summary>
    public interface INetDaemonApp : INetDaemonAppBase
    {
        /// <summary>
        ///     The apps actions to run on state changes
        /// </summary>
        public ConcurrentDictionary<string, (string pattern, Func<string, EntityState?, EntityState?, Task> action)> StateCallbacks { get; }

        /// <summary>
        ///     Selects one or more camera entities to do action on
        /// </summary>
        /// <param name="entityIds">Entity unique id:s</param>
        ICamera Camera(params string[] entityIds);

        /// <summary>
        ///     Selects one or more cameras entities to do action on
        /// </summary>
        /// <param name="entityIds">Entity unique id:s</param>
        ICamera Cameras(IEnumerable<string> entityIds);

        /// <summary>
        ///     Selects one or more cameras entities to do action on using lambda
        /// </summary>
        /// <param name="func">The lambda expression selecting mediaplayers</param>
        ICamera Cameras(Func<IEntityProperties, bool> func);

        /// <summary>
        ///     Cancels the current listen state operation
        /// </summary>
        /// <param name="id">The unique id provided in ListenState</param>
        ///
        void CancelListenState(string id);

        /// <summary>
        ///     Delays until state changes
        /// </summary>
        /// <param name="entityIds">The entities to wait for state changes</param>
        /// <param name="to">The state change to, or null if any state</param>
        /// <param name="from">The state changed from or null if any state</param>
        /// <param name="allChanges">Get all changed, even only attribute changes</param>
        IDelayResult DelayUntilStateChange(IEnumerable<string> entityIds, object? to = null, object? from = null, bool allChanges = false);

        /// <summary>
        ///     Delays until state changes
        /// </summary>
        /// <param name="entityId">The unique id of the enitty to wait for state changes</param>
        /// <param name="to">The state change to, or null if any state</param>
        /// <param name="from">The state changed from or null if any state</param>
        /// <param name="allChanges">Get all changed, even only attribute changes</param>
        IDelayResult DelayUntilStateChange(string entityId, object? to = null, object? from = null, bool allChanges = false);

        /// <summary>
        ///     Delays until state changes using lambda expression to check states
        /// </summary>
        /// <param name="entityIds">The entities to wait for state changes</param>
        /// <param name="stateFunc">Lambda expression to select state changes</param>
        IDelayResult DelayUntilStateChange(IEnumerable<string> entityIds, Func<EntityState?, EntityState?, bool> stateFunc);
        /// <summary>
        ///     Selects one or more entities to do action on
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        IEntity Entities(IEnumerable<string> entityId);

        /// <summary>
        ///     Selects one or more entities to do action on using lambda
        /// </summary>
        /// <param name="func">The lambda expression for selecting entities</param>
        IEntity Entities(Func<IEntityProperties, bool> func);

        /// <summary>
        ///     Selects one or more entities to do action on
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        IEntity Entity(params string[] entityId);

        // FLUENT API
        /// <summary>
        ///     Selects one or more events to do action on
        /// </summary>
        /// <param name="eventParams">Events</param>
        IFluentEvent Event(params string[] eventParams);

        /// <summary>
        ///     Selects one or more events to do action on
        /// </summary>
        /// <param name="eventParams">Events</param>
        IFluentEvent Events(IEnumerable<string> eventParams);

        /// <summary>
        ///     Selects the events to do actions on using lambda
        /// </summary>
        /// <param name="func">The lambda expression selecting event</param>
        IFluentEvent Events(Func<FluentEventProperty, bool> func);

        /// <summary>
        ///     Selects one or more input select to do action on
        /// </summary>
        /// <param name="inputSelectParams">Events</param>
        IFluentInputSelect InputSelect(params string[] inputSelectParams);

        /// <summary>
        ///     Selects one or more input selects to do action on
        /// </summary>
        /// <param name="inputSelectParams">Events</param>
        IFluentInputSelect InputSelects(IEnumerable<string> inputSelectParams);

        /// <summary>
        ///     Selects the input selects to do actions on using lambda
        /// </summary>
        /// <param name="func">The lambda expression selecting input select</param>
        IFluentInputSelect InputSelects(Func<IEntityProperties, bool> func);

        /// <summary>
        ///     Listen to state change
        /// </summary>
        /// <param name="ev">The event to listen to</param>
        /// <param name="action">The action to call when event fires</param>
        void ListenEvent(string ev,
            Func<string, dynamic?, Task> action);

        /// <summary>
        ///     Listen to event state
        /// </summary>
        /// <param name="funcSelector">Using lambda expression to select event</param>
        /// <param name="action">The action to call when event fires</param>
        void ListenEvent(Func<FluentEventProperty, bool> funcSelector,
                         Func<string, dynamic, Task> action);

        /// <summary>
        ///     Listen to service calls
        /// </summary>
        /// <param name="domain">The domain of the service call</param>
        /// <param name="service">The service being called</param>
        /// <param name="action">The action to perform when service is called</param>
        void ListenServiceCall(string domain, string service,
            Func<dynamic?, Task> action);

        /// <summary>
        ///     Listen to statechange
        /// </summary>
        /// <param name="pattern">Match pattern, entity_id or domain</param>
        /// <param name="action">The func to call when matching</param>
        /// <returns>Returns a guid that can be used to cancel the event listening with CancelListenState</returns>
        /// <remarks>
        ///     The callback function is
        ///         - EntityId
        ///         - newEvent
        ///         - oldEvent
        /// </remarks>
        string? ListenState(string pattern,
            Func<string, EntityState?, EntityState?, Task> action);
        /// <summary>
        ///     Selects one or more media player entities to do action on
        /// </summary>
        /// <param name="entityIds">Entity unique id:s</param>
        IMediaPlayer MediaPlayer(params string[] entityIds);

        /// <summary>
        ///     Selects one or more media player entities to do action on
        /// </summary>
        /// <param name="entityIds">Entity unique id:s</param>
        IMediaPlayer MediaPlayers(IEnumerable<string> entityIds);

        /// <summary>
        ///     Selects one or more media player entities to do action on using lambda
        /// </summary>
        /// <param name="func">The lambda expression selecting mediaplayers</param>
        IMediaPlayer MediaPlayers(Func<IEntityProperties, bool> func);

        /// <summary>
        ///     Runs one or more scripts
        /// </summary>
        /// <param name="entityIds">The unique id:s of the script</param>
        IScript RunScript(params string[] entityIds);
    }

    /// <summary>
    ///     Shared features in both Reactive and async/await models
    /// </summary>
    public interface INetDaemonAppBase : INetDaemonInitialableApp, IAsyncDisposable, IEquatable<INetDaemonAppBase>
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
        ///     Gets or sets a flag indicating whether this app is enabled.
        ///     This property property can be controlled from Home Assistant.
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
        ///     Use text-to-speech to speak a message
        /// </summary>
        /// <param name="entityId">Unique id of the media player the speech should play</param>
        /// <param name="message">The message that will be spoken</param>
        void Speak(string entityId, string message);
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
        ///     Schedule actions to fire in different time
        /// </summary>
        IScheduler Scheduler { get; }

        /// <summary>
        ///     All current states for all known entities
        /// </summary>
        /// <remarks>
        ///     All states are read and cached at startup. Every state change updates the
        ///     states. There can be a small risk that the state is not updated
        ///     exactly when it happens but it should be fine. The SetState function
        ///     updates the state before sending.
        /// </remarks>
        IEnumerable<EntityState> State { get; }

        /// <summary>
        ///     Calls a service
        /// </summary>
        /// <param name="domain">The domain of the service</param>
        /// <param name="service">The service being called</param>
        /// <param name="data">Any data that the service requires</param>
        /// <param name="waitForResponse">If we should wait for the service to get response from Home Assistant or send/forget scenario</param>
        Task CallServiceAsync(string domain, string service, dynamic? data = null, bool waitForResponse = false);

        /// <summary>
        ///     Get application instance by application instance id
        /// </summary>
        /// <param name="appInstanceId">The identity of the app instance</param>
        NetDaemonApp? GetApp(string appInstanceId);

        /// <summary>
        ///     Loads persistent data from unique id
        /// </summary>
        /// <param name="id">Unique Id of the data</param>
        /// <returns>The data persistent or null if not exists</returns>
        ValueTask<T> GetDataAsync<T>(string id);

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
        /// <returns></returns>
        /// <remarks>
        ///     Restores the state of the storage object.!--
        ///     Todo: in the future also the state of tagged properties
        ///
        ///     It is implemented async so state will be lazy saved
        /// </remarks>
        Task RestoreAppStateAsync();

        /// <summary>
        ///     Saves the app state
        /// </summary>
        /// <remarks>
        ///     Saves the state of the storage object.!--
        ///     Todo: in the future also the state of tagged properties
        ///
        ///     It is implemented async so state will be lazy saved
        /// </remarks>
        void SaveAppState();

        /// <summary>
        /// Start the application, normally implemented by the base class
        /// </summary>
        /// <param name="daemon"></param>
        Task StartUpAsync(INetDaemon daemon);
    }
}