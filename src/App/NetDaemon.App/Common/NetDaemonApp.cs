using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    /// <summary>
    ///     Base class för all NetDaemon apps
    /// </summary>
    public abstract class NetDaemonApp : NetDaemonAppBase, INetDaemonApp, INetDaemonCommon
    {
        /// <inheritdoc/>
        public IScheduler Scheduler => _daemon?.Scheduler ??
            throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

        /// <inheritdoc/>
        public IEnumerable<EntityState> State => _daemon?.State ??
            throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
 
        /// <inheritdoc/>
        public Task CallServiceAsync(string domain, string service, dynamic? data = null, bool waitForResponse = false)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.CallServiceAsync(domain, service, data, waitForResponse);
        }

        /// <inheritdoc/>
        public ICamera Camera(params string[] entityIds)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Camera(this, entityIds);
        }

        /// <inheritdoc/>
        public ICamera Cameras(IEnumerable<string> entityIds)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Cameras(this, entityIds);
        }

        /// <inheritdoc/>
        public ICamera Cameras(Func<IEntityProperties, bool> func)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Cameras(this, func);
        }

        /// <inheritdoc/>
        public void CancelListenState(string id) => _daemon?.CancelListenState(id);

        /// <inheritdoc/>
        public IDelayResult DelayUntilStateChange(IEnumerable<string> entityIds, object? to = null, object? from = null, bool allChanges = false)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.DelayUntilStateChange(entityIds, to, from, allChanges);
        }

        /// <inheritdoc/>
        public IDelayResult DelayUntilStateChange(string entityId, object? to = null, object? from = null, bool allChanges = false)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.DelayUntilStateChange(entityId, to, from, allChanges);
        }

        /// <inheritdoc/>
        public IDelayResult DelayUntilStateChange(IEnumerable<string> entityIds, Func<EntityState?, EntityState?, bool> stateFunc)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.DelayUntilStateChange(entityIds, stateFunc);
        }

        /// <inheritdoc/>
        public IEntity Entities(Func<IEntityProperties, bool> func)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Entities(this, func);
        }

        /// <inheritdoc/>
        public IEntity Entities(IEnumerable<string> entityIds)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Entities(this, entityIds);
        }

        /// <inheritdoc/>
        public IEntity Entity(params string[] entityId)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Entity(this, entityId);
        }

        /// <inheritdoc/>
        public IFluentEvent Event(params string[] eventParams) => _daemon?.Event(this, eventParams) ??
            throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

        /// <inheritdoc/>
        public IFluentEvent Events(Func<FluentEventProperty, bool> func) => _daemon?.Events(this, func) ??
            throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

        /// <inheritdoc/>
        public IFluentEvent Events(IEnumerable<string> eventParams) => _daemon?.Events(this, eventParams) ??
            throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

        /// <inheritdoc/>
        public NetDaemonApp? GetApp(string appInstanceId)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.GetApp(appInstanceId);
        }

        /// <inheritdoc/>
        public async ValueTask<T> GetDataAsync<T>(string id)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return await _daemon!.GetDataAsync<T>(id).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public EntityState? GetState(string entityId) => _daemon?.GetState(entityId);

        /// <inheritdoc/>
        public IFluentInputSelect InputSelect(params string[] inputSelectParams)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.InputSelect(this, inputSelectParams);
        }

        /// <inheritdoc/>
        public IFluentInputSelect InputSelects(IEnumerable<string> inputSelectParams)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.InputSelects(this, inputSelectParams);
        }

        /// <inheritdoc/>
        public IFluentInputSelect InputSelects(Func<IEntityProperties, bool> func)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.InputSelects(this, func);
        }

        /// <inheritdoc/>
        public void ListenEvent(string ev, Func<string, dynamic?, Task> action) => _daemon?.ListenEvent(ev, action);

        /// <inheritdoc/>
        public void ListenEvent(Func<FluentEventProperty, bool> funcSelector, Func<string, dynamic, Task> func) =>
                _daemon?.ListenEvent(funcSelector, func);

        /// <inheritdoc/>
        public void ListenServiceCall(string domain, string service, Func<dynamic?, Task> action) =>
                _daemon?.ListenServiceCall(domain, service, action);

        /// <inheritdoc/>
        public string? ListenState(string pattern, Func<string, EntityState?, EntityState?, Task> action) => _daemon?.ListenState(pattern, action);

    
        /// <inheritdoc/>
        public IMediaPlayer MediaPlayer(params string[] entityIds)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.MediaPlayer(this, entityIds);
        }

        /// <inheritdoc/>
        public IMediaPlayer MediaPlayers(IEnumerable<string> entityIds)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.MediaPlayers(this, entityIds);
        }

        /// <inheritdoc/>
        public IMediaPlayer MediaPlayers(Func<IEntityProperties, bool> func)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.MediaPlayers(this, func);
        }



        /// <inheritdoc/>
        public IScript RunScript(params string[] entityIds)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.RunScript(this, entityIds);
        }



        /// <inheritdoc/>
        public Task SaveDataAsync<T>(string id, T data)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.SaveDataAsync<T>(id, data);
        }

        /// <inheritdoc/>
        public async Task<bool> SendEvent(string eventId, dynamic? data = null)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return await _daemon!.SendEvent(eventId, data).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<EntityState?> SetStateAsync(string entityId, dynamic state, params (string name, object val)[] attributes)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return await _daemon!.SetStateAsync(entityId, state, attributes).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Speak(string entityId, string message)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            _daemon!.Speak(entityId, message);
        }
   
    }
}