using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    public class NetDaemonApp : INetDaemonApp, INetDaemon
    {
        private INetDaemon? _daemon;

        /// <summary>
        ///     Logger to use for logging to the console
        /// </summary>
        public ILogger Logger { get; private set; }

        /// <summary>
        ///     Schedule actions to fire in different time
        /// </summary>
        public IScheduler Scheduler => _daemon?.Scheduler;

        /// <summary>
        ///     All current states for all known entities
        /// </summary>
        /// <remarks>
        ///     All states are read and cached at startup. Every state change updates the
        ///     states. There can be a small risk that the state is not updated
        ///     exactly when it happens but it should be fine. The SetState function
        ///     updates the state before sending.
        /// </remarks>
        public IEnumerable<EntityState> State => _daemon.State;

        /// <summary>
        ///     Calls a service
        /// </summary>
        /// <param name="domain">The domain of the service</param>
        /// <param name="service">The service being called</param>
        /// <param name="data">Any data that the service requires</param>
        /// <param name="waitForResponse">If we should wait for the service to get response from Home Assistant or send/forget scenario</param>
        public async Task CallService(string domain, string service, dynamic? data = null, bool waitForResponse = false)
        {
            if (_daemon != null)
            {
                await _daemon.CallService(domain, service, data, waitForResponse);
            }
        }

        /// <summary>
        ///     Selects one or more entities to do action on using lambda
        /// </summary>
        /// <param name="func">The lambda expression for selecting entities</param>
        public IEntity Entities(Func<IEntityProperties, bool> func)
        {
            if (_daemon != null)
            {
                return _daemon.Entities(func);
            }

            return null;
        }

        /// <summary>
        ///     Selects one or more entities to do action on
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        public IEntity Entities(IEnumerable<string> entityIds)
        {
            if (_daemon != null)
            {
                return _daemon.Entities(entityIds);
            }

            return null;
        }

        /// <summary>
        ///     Selects one or more entities to do action on
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        public IEntity Entity(params string[] entityId)
        {
            if (_daemon != null)
            {
                return _daemon.Entity(entityId);
            }

            return null;
        }

        /// <summary>
        ///     Selects one or more events to do action on
        /// </summary>
        /// <param name="eventParams">Events</param>
        public IFluentEvent Event(params string[] eventParams) => _daemon.Event(eventParams);

        /// <summary>
        ///     Selects the events to do actions on using lambda
        /// </summary>
        /// <param name="func">The lambda expression selecting event</param>
        public IFluentEvent Events(Func<FluentEventProperty, bool> func) => _daemon.Events(func);

        /// <summary>
        ///     Selects one or more events to do action on
        /// </summary>
        /// <param name="eventParams">Events</param>
        public IFluentEvent Events(IEnumerable<string> eventParams) => _daemon.Events(eventParams);

        /// <summary>
        ///     Gets current state for the entity
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        /// <returns></returns>
        public EntityState? GetState(string entityId) => _daemon?.GetState(entityId);

        public virtual Task InitializeAsync()
        {
            // Do nothing as default
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Selects one or more light entities to do action on
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        public ILight Light(params string[] entity)
        {
            if (_daemon != null)
            {
                return _daemon.Light(entity);
            }

            return null;
        }

        /// <summary>
        ///     Listen to state change
        /// </summary>
        /// <param name="ev">The event to listen to</param>
        /// <param name="action">The action to call when event fires</param>
        public void ListenEvent(string ev, Func<string, dynamic, Task> action) => _daemon?.ListenEvent(ev, action);

        /// <summary>
        ///     Listen to event state
        /// </summary>
        /// <param name="funcSelector">Using lambda expression to select event</param>
        /// <param name="action">The action to call when event fires</param>
        public void ListenEvent(Func<FluentEventProperty, bool> funcSelector, Func<string, dynamic, Task> func) =>
                _daemon?.ListenEvent(funcSelector, func);

        /// <summary>
        ///     Listen to service calls
        /// </summary>
        /// <param name="domain">The domain of the service call</param>
        /// <param name="service">The service being called</param>
        /// <param name="app">The application instance</param>
        /// <param name="action">The action to perform when service is called</param>
        public void ListenServiceCall(string domain, string service, Func<dynamic, Task> action) =>
                _daemon?.ListenServiceCall(domain, service, action);

        /// <summary>
        ///     Listen for state changes and call a function when state changes
        /// </summary>
        /// <remarks>
        ///     Make function like
        ///     <code>
        ///     ListenState("binary_sensor.pir", async (string entityId, EntityState newState, EntityState oldState) =>
        ///     {
        ///         await Task.Delay(1000);// Insert some code
        ///     });
        ///     </code>
        ///     Valid patterns are:
        ///         light.thelight      - En entity id
        ///         light               - No dot means the whole domain
        ///         empty               - All events
        /// </remarks>
        /// <param name="pattern">Ientity id or domain</param>
        /// <param name="action">The action to call when state is changed, see remarks</param>
        public void ListenState(string pattern, Func<string, EntityState?, EntityState?, Task> action) => _daemon?.ListenState(pattern, action);

        public void Log(string message, LogLevel level = LogLevel.Information) => Logger.Log(level, message);

        public void Log(string message, Exception exception, LogLevel level = LogLevel.Information) => Logger.Log(level, exception, message);

        /// <summary>
        ///     Selects one or more media player entities to do action on
        /// </summary>
        /// <param name="entityIds">Entity unique id:s</param>
        public IMediaPlayer MediaPlayer(params string[] entityIds)
        {
            if (_daemon != null)
            {
                return _daemon.MediaPlayer(entityIds);
            }

            return null;
        }

        /// <summary>
        ///     Selects one or more media player entities to do action on
        /// </summary>
        /// <param name="entityIds">Entity unique id:s</param>
        public IMediaPlayer MediaPlayers(IEnumerable<string> entityIds)
        {
            if (_daemon != null)
            {
                return _daemon.MediaPlayers(entityIds);
            }

            return null;
        }

        /// <summary>
        ///     Selects one or more media player entities to do action on using lambda
        /// </summary>
        /// <param name="func">The lambda expression selecting mediaplayers</param>
        public IMediaPlayer MediaPlayers(Func<IEntityProperties, bool> func)
        {
            if (_daemon != null)
            {
                return _daemon.MediaPlayers(func);
            }

            return null;
        }

        /// <summary>
        ///     Runs one or more scripts
        /// </summary>
        /// <param name="entityIds">The unique id:s of the script</param>
        public IScript RunScript(params string[] entityIds)
        {
            if (_daemon != null)
            {
                return _daemon.RunScript(entityIds);
            }

            return null;
        }

        /// <summary>
        ///     Sends a custom event
        /// </summary>
        /// <param name="eventId">Any identity of the event</param>
        /// <param name="data">Any data sent with the event</param>
        public async Task<bool> SendEvent(string eventId, dynamic? data = null)
        {
            if (_daemon != null)
            {
                return await _daemon.SendEvent(eventId, data);
            }

            return false;
        }

        /// <summary>
        ///     Set entity state
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        /// <param name="state">The state being set</param>
        /// <param name="attributes">Name/Value pair of the attribute</param>
        public async Task<EntityState?> SetState(string entityId, dynamic state, params (string name, object val)[] attributes)
        {
            if (_daemon != null)
            {
                return await _daemon.SetState(entityId, state, attributes);
            }

            return null;
        }

        /// <summary>
        ///     Use text-to-speech to speak a message
        /// </summary>
        /// <param name="entityId">Unique id of the media player the speech should play</param>
        /// <param name="message">The message that will be spoken</param>
        public async Task Speak(string entityId, string message)
        {
            if (_daemon != null)
            {
                await _daemon.Speak(entityId, message);
            }
        }

        /// <summary>
        ///     The startup method to do basic initialization, called by the daemon
        /// </summary>
        /// <param name="daemon">Netdaemon instance</param>
        public virtual Task StartUpAsync(INetDaemon daemon)
        {
            _daemon = daemon;
            Logger = daemon.Logger;

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Toggle entity who support the service call
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        /// <param name="attributes">Name/Value pair of the attribute</param>
        public async Task ToggleAsync(string entityId, params (string name, object val)[] attributes)
        {
            if (_daemon != null)
            {
                await _daemon.ToggleAsync(entityId, attributes);
            }
        }

        /// <summary>
        ///     Turn off entity who support the service call
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        /// <param name="attributes">Name/Value pair of the attribute</param>
        public async Task TurnOffAsync(string entityId, params (string name, object val)[] attributes)
        {
            if (_daemon != null)
            {
                await _daemon.TurnOffAsync(entityId, attributes);
            }
        }

        /// <summary>
        ///     Turn on entity who support the service call
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        /// <param name="attributes">Name/Value pair of the attribute</param>
        public async Task TurnOnAsync(string entityId, params (string name, object val)[] attributes)
        {
            if (_daemon != null)
            {
                await _daemon.TurnOnAsync(entityId, attributes);
            }
        }
    }
}