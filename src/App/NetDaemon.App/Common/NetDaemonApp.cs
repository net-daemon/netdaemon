using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    public class NetDaemonApp : INetDaemonApp, INetDaemon
    {
        private INetDaemon? _daemon;
#pragma warning disable 4014
        public virtual Task StartUpAsync(INetDaemon daemon)
        {
            _daemon = daemon;
            Logger = daemon.Logger;

            return Task.CompletedTask;
        }

        public virtual Task InitializeAsync()
        {
            // Do nothing as default
            return Task.CompletedTask;
        }

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

        public async Task TurnOnAsync(string entityId, params (string name, object val)[] attributeNameValuePair)
        {
            if (_daemon != null) await _daemon.TurnOnAsync(entityId, attributeNameValuePair);
        }

        public async Task TurnOffAsync(string entityId, params (string name, object val)[] attributes)
        {
            if (_daemon != null) await _daemon.TurnOffAsync(entityId, attributes);
        }

        public async Task ToggleAsync(string entityId, params (string name, object val)[] attributes)
        {
            if (_daemon != null) await _daemon.ToggleAsync(entityId, attributes);
        }

        public async Task<EntityState?> SetState(string entityId, dynamic state, params (string name, object val)[] attributes)
        {
            if (_daemon != null) return await _daemon.SetState(entityId, state, attributes);
            return null;
        }

        public EntityState? GetState(string entityId)
        {
            return _daemon?.GetState(entityId);
        }

        public IEntity Entity(params string[] entityId)
        {
            if (_daemon != null) return _daemon.Entity(entityId);

            return null;
        }

        public IEntity Entities(Func<IEntityProperties, bool> func)
        {
            if (_daemon != null) return _daemon.Entities(func);

            return null;
        }

        public ILight Light(params string[] entity)
        {
            if (_daemon != null) return _daemon.Light(entity);

            return null;
        }

        public IEnumerable<EntityState> State => _daemon.State;
        public IScheduler Scheduler => _daemon?.Scheduler;

        public ILogger? Logger { get; private set; }

        public void Log(string message, LogLevel level = LogLevel.Information)
        {
            Logger.Log(level, message);
        }

        public void Log(string message, Exception exception, LogLevel level = LogLevel.Information)
        {
            Logger.Log(level, exception, message);
        }

        //public IAction Action => _daemon.Action;

    }
}