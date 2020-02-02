using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    public class NetDaemonApp : INetDaemonApp, INetDaemon
    {
        private INetDaemon? _daemon;
        public ILogger Logger { get; private set; }

        public IScheduler Scheduler => _daemon?.Scheduler;

        public IEnumerable<EntityState> State => _daemon.State;

        public async Task CallService(string domain, string service, dynamic? data = null, bool waitForResponse = false)
        {
            if (_daemon != null) await _daemon.CallService(domain, service, data, waitForResponse);
        }

        public IEntity Entities(Func<IEntityProperties, bool> func)
        {
            if (_daemon != null) return _daemon.Entities(func);

            return null;
        }

        public IEntity Entity(params string[] entityId)
        {
            if (_daemon != null) return _daemon.Entity(entityId);

            return null;
        }

        public IFluentEvent Event(params string[] eventParams) => _daemon.Event(eventParams);
        public IFluentEvent Events(Func<FluentEventProperty, bool> func) => _daemon.Events(func);

        public EntityState? GetState(string entityId)
        {
            return _daemon?.GetState(entityId);
        }

        public virtual Task InitializeAsync()
        {
            // Do nothing as default
            return Task.CompletedTask;
        }

        public ILight Light(params string[] entity)
        {
            if (_daemon != null) return _daemon.Light(entity);

            return null;
        }

        public void ListenEvent(string ev, Func<string, dynamic, Task> action) => _daemon?.ListenEvent(ev, action);
        public void ListenEvent(Func<FluentEventProperty, bool> funcSelector, Func<string, dynamic, Task> func) =>
                _daemon?.ListenEvent(funcSelector, func);

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
        public void ListenState(string pattern, Func<string, EntityState?, EntityState?, Task> action)
        {
            _daemon?.ListenState(pattern, action);
        }

        public void Log(string message, LogLevel level = LogLevel.Information)
        {
            Logger.Log(level, message);
        }

        public void Log(string message, Exception exception, LogLevel level = LogLevel.Information)
        {
            Logger.Log(level, exception, message);
        }

        public IMediaPlayer MediaPlayer(params string[] entity)
        {
            if (_daemon != null) return _daemon.MediaPlayer(entity);

            return null;
        }

        public IMediaPlayer MediaPlayers(Func<IEntityProperties, bool> func)
        {
            if (_daemon != null) return _daemon.MediaPlayers(func);

            return null;
        }

        public IScript RunScript(params string[] entityIds)
        {
            if (_daemon != null) return _daemon.RunScript(entityIds);

            return null;
        }

        public async Task<EntityState?> SetState(string entityId, dynamic state, params (string name, object val)[] attributes)
        {
            if (_daemon != null) return await _daemon.SetState(entityId, state, attributes);
            return null;
        }

        public virtual Task StartUpAsync(INetDaemon daemon)
        {
            _daemon = daemon;
            Logger = daemon.Logger;

            return Task.CompletedTask;
        }
        public async Task ToggleAsync(string entityId, params (string name, object val)[] attributes)
        {
            if (_daemon != null) await _daemon.ToggleAsync(entityId, attributes);
        }

        public async Task TurnOffAsync(string entityId, params (string name, object val)[] attributes)
        {
            if (_daemon != null) await _daemon.TurnOffAsync(entityId, attributes);
        }

        public async Task TurnOnAsync(string entityId, params (string name, object val)[] attributeNameValuePair)
        {
            if (_daemon != null) await _daemon.TurnOnAsync(entityId, attributeNameValuePair);
        }
        //public IAction Action => _daemon.Action;

    }
}