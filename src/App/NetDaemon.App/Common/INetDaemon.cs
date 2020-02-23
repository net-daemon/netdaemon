using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    /// <summary>
    /// Interface that all NetDaemon apps needs to implement
    /// </summary>
    public interface INetDaemonApp
    {
        /// <summary>
        /// Start the application, normally implemented by the base class
        /// </summary>
        /// <param name="daemon"></param>
        Task StartUpAsync(INetDaemon daemon);

        /// <summary>
        /// Init the application, is called by the NetDaemon after startup
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        ///     Access stateful data
        /// </summary>
        /// <remarks>
        ///     The dynamic setter will automatically persist the whole storage object
        /// </remarks>
        dynamic Storage { get; }

        /// <summary>
        ///     Unique id of the application
        /// </summary>
        public string? Id { get; set; }

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

    }

    /// <summary>
    ///     The interface that interacts with the daemon main logic
    /// </summary>
    public interface INetDaemon
    {
        /// <summary>
        ///     Logger to use
        /// </summary>
        ILogger? Logger { get; }

        /// <summary>
        ///     Listen to statechange
        /// </summary>
        /// <param name="pattern">Match pattern, entity_id or domain</param>
        /// <param name="action">The func to call when matching</param>
        /// <remarks>
        ///     The callback function is
        ///         - EntityId
        ///         - newEvent
        ///         - oldEvent
        /// </remarks>
        void ListenState(string pattern,
            Func<string, EntityState?, EntityState?, Task> action);

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
        ///     Turn on entity who support the service call
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        /// <param name="attributes">Name/Value pair of the attribute</param>

        Task TurnOnAsync(string entityId, params (string name, object val)[] attributes);

        /// <summary>
        ///     Turn off entity who support the service call
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        /// <param name="attributes">Name/Value pair of the attribute</param>
        Task TurnOffAsync(string entityId, params (string name, object val)[] attributes);

        /// <summary>
        ///     Toggle entity who support the service call
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        /// <param name="attributes">Name/Value pair of the attribute</param>
        Task ToggleAsync(string entityId, params (string name, object val)[] attributes);

        /// <summary>
        ///     Set entity state
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        /// <param name="state">The state being set</param>
        /// <param name="attributes">Name/Value pair of the attribute</param>
        Task<EntityState?> SetState(string entityId, dynamic state, params (string name, object val)[] attributes);

        /// <summary>
        ///     Calls a service
        /// </summary>
        /// <param name="domain">The domain of the service</param>
        /// <param name="service">The service being called</param>
        /// <param name="data">Any data that the service requires</param>
        /// <param name="waitForResponse">If we should wait for the service to get response from Home Assistant or send/forget scenario</param>
        Task CallService(string domain, string service, dynamic? data = null, bool waitForResponse = false);

        /// <summary>
        ///     Sends a custom event
        /// </summary>
        /// <param name="eventId">Any identity of the event</param>
        /// <param name="data">Any data sent with the event</param>
        Task<bool> SendEvent(string eventId, dynamic? data = null);

        /// <summary>
        ///     Gets current state for the entity
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        /// <returns></returns>
        EntityState? GetState(string entityId);

        /// <summary>
        ///     Selects one or more entities to do action on
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        IEntity Entity(params string[] entityId);

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
        ///     Selects one or more light entities to do action on
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        ILight Light(params string[] entityId);

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
        ///     Schedule actions to fire in different time
        /// </summary>
        IScheduler Scheduler { get; }

        /// <summary>
        ///     Use text-to-speech to speak a message
        /// </summary>
        /// <param name="entityId">Unique id of the media player the speech should play</param>
        /// <param name="message">The message that will be spoken</param>
        void Speak(string entityId, string message);

        /// <summary>
        ///     Saves any data with unique id, data have to be json serializable
        /// </summary>
        /// <param name="id">Unique id for all apps</param>
        /// <param name="data">Dynamic data being saved</param>
        Task SaveDataAsync<T>(string id, T data);

        /// <summary>
        ///     Loads persistent data from unique id
        /// </summary>
        /// <param name="id">Unique Id of the data</param>
        /// <returns>The data persistent or null if not exists</returns>
        ValueTask<T> GetDataAsync<T>(string id);
    }

}
