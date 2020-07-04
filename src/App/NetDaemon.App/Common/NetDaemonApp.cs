using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    /// <summary>
    ///     Base class för all NetDaemon apps
    /// </summary>
    public abstract class NetDaemonApp : NetDaemonAppBase, INetDaemonApp, INetDaemonCommon
    {
        private readonly IList<(string pattern, Func<string, dynamic, Task> action)> _eventCallbacks =
                                            new List<(string pattern, Func<string, dynamic, Task> action)>();

        private readonly List<(Func<FluentEventProperty, bool>, Func<string, dynamic, Task>)> _eventFunctionSelectorCallbacks =
                    new List<(Func<FluentEventProperty, bool>, Func<string, dynamic, Task>)>();
        private readonly ConcurrentDictionary<string, (string pattern, Func<string, EntityState?, EntityState?, Task> action)> _stateCallbacks =
                                    new ConcurrentDictionary<string, (string pattern, Func<string, EntityState?, EntityState?, Task> action)>();

        /// <summary>
        ///     All actions being performed for named events
        /// </summary>
        public IList<(string pattern, Func<string, dynamic, Task> action)> EventCallbacks => _eventCallbacks;

        /// <summary>
        ///     All actions being performed for lambda selected events
        /// </summary>
        public List<(Func<FluentEventProperty, bool>, Func<string, dynamic, Task>)> EventFunctionCallbacks => _eventFunctionSelectorCallbacks;

        /// <inheritdoc/>
        public IScheduler Scheduler => _daemon?.Scheduler ??
            throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
        /// <inheritdoc/>
        public IEnumerable<EntityState> State => _daemon?.State ??
            throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

        /// <inheritdoc/>
        public ConcurrentDictionary<string, (string pattern, Func<string, EntityState?, EntityState?, Task> action)>
            StateCallbacks => _stateCallbacks;

        // Used for testing
        internal ConcurrentDictionary<string, (string pattern, Func<string, EntityState?, EntityState?, Task> action)> InternalStateActions => _stateCallbacks;

        /// <inheritdoc/>
        public Task CallService(string domain, string service, dynamic? data = null, bool waitForResponse = false)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.CallServiceAsync(domain, service, data, waitForResponse);
        }

        /// <inheritdoc/>
        public ICamera Camera(params string[] entityIds)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Camera(this, entityIds);
        }

        /// <inheritdoc/>
        public ICamera Cameras(IEnumerable<string> entityIds)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Cameras(this, entityIds);
        }

        /// <inheritdoc/>
        public ICamera Cameras(Func<IEntityProperties, bool> func)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Cameras(this, func);
        }

        /// <inheritdoc/>
        public void CancelListenState(string id)
        {
            // Remove and ignore if not exist
            _stateCallbacks.Remove(id, out _);
        }

        /// <inheritdoc/>
        public IDelayResult DelayUntilStateChange(string entityId, object? to = null, object? from = null, bool allChanges = false) =>
            DelayUntilStateChange(new string[] { entityId }, to, from, allChanges);

        /// <inheritdoc/>
        public IDelayResult DelayUntilStateChange(IEnumerable<string> entityIds, object? to = null, object? from = null, bool allChanges = false)
        {
            // Use TaskCompletionSource to simulate a task that we can control
            var taskCompletionSource = new TaskCompletionSource<bool>();
            var result = new DelayResult(taskCompletionSource, this);

            foreach (var entityId in entityIds)
            {
                result.StateSubscriptions.Add(ListenState(entityId, (entityIdInn, newState, oldState) =>
                {
                    if (to != null)
                        if ((dynamic)to != newState?.State)
                            return Task.CompletedTask;

                    if (from != null)
                        if ((dynamic)from != oldState?.State)
                            return Task.CompletedTask;

                    // If we don´t accept all changes in the state change
                    // and we do not have a state change so return
                    if (newState?.State == oldState?.State && !allChanges)
                        return Task.CompletedTask;

                    // If we reached this far we should complete task!
                    taskCompletionSource.SetResult(true);
                    // Also cancel all other ongoing state change subscriptions
                    result.Cancel();

                    return Task.CompletedTask;
                })!);
            }

            return result;
        }

        /// <inheritdoc/>
        public IDelayResult DelayUntilStateChange(IEnumerable<string> entityIds, Func<EntityState?, EntityState?, bool> stateFunc)
        {
            // Use TaskCompletionSource to simulate a task that we can control
            var taskCompletionSource = new TaskCompletionSource<bool>();
            var result = new DelayResult(taskCompletionSource, this);

            foreach (var entityId in entityIds)
            {
                result.StateSubscriptions.Add(ListenState(entityId, (entityIdInn, newState, oldState) =>
                {
                    try
                    {
                        if (!stateFunc(newState, oldState))
                            return Task.CompletedTask;
                    }
                    catch (Exception e)
                    {
                        Logger.LogWarning(e, "Failed to evaluate function");
                        return Task.CompletedTask;
                    }

                    // If we reached this far we should complete task!
                    taskCompletionSource.SetResult(true);
                    // Also cancel all other ongoing state change subscriptions
                    result.Cancel();

                    return Task.CompletedTask;
                })!);
            }

            return result;
        }

        /// <summary>
        ///     Implements the async dispose pattern
        /// </summary>
        public async override ValueTask DisposeAsync()
        {
            _stateCallbacks.Clear();
            _eventCallbacks.Clear();
            _eventFunctionSelectorCallbacks.Clear();
    
            await base.DisposeAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public IEntity Entities(Func<IEntityProperties, bool> func)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Entities(this, func);
        }

        /// <inheritdoc/>
        public IEntity Entities(IEnumerable<string> entityIds)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Entities(this, entityIds);
        }

        /// <inheritdoc/>
        public IEntity Entity(params string[] entityId)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Entity(this, entityId);
        }

        /// <inheritdoc/>
        public IFluentEvent Event(params string[] eventParams) => new FluentEventManager(eventParams, this);

        /// <inheritdoc/>
        public IFluentEvent Events(Func<FluentEventProperty, bool> func) => new FluentEventManager(func, this);

        /// <inheritdoc/>
        public IFluentEvent Events(IEnumerable<string> eventParams) => new FluentEventManager(eventParams, this);

        /// <inheritdoc/>
        public async ValueTask<T?> GetDataAsync<T>(string id) where T : class
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return await _daemon!.GetDataAsync<T>(id).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public EntityState? GetState(string entityId) => _daemon?.GetState(entityId);

        /// <inheritdoc/>
        public IFluentInputSelect InputSelect(params string[] inputSelectParams)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.InputSelect(this, inputSelectParams);
        }

        /// <inheritdoc/>
        public IFluentInputSelect InputSelects(IEnumerable<string> inputSelectParams)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.InputSelects(this, inputSelectParams);
        }

        /// <inheritdoc/>
        public IFluentInputSelect InputSelects(Func<IEntityProperties, bool> func)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.InputSelects(this, func);
        }

        /// <inheritdoc/>
        public void ListenEvent(string ev, Func<string, dynamic, Task> action) => _eventCallbacks.Add((ev, action));

        /// <inheritdoc/>
        public void ListenEvent(Func<FluentEventProperty, bool> funcSelector, Func<string, dynamic, Task> func) => _eventFunctionSelectorCallbacks.Add((funcSelector, func));

        /// <inheritdoc/>
        public string? ListenState(string pattern,
            Func<string, EntityState?, EntityState?, Task> action)
        {
            // Use guid as unique id but will externally use string so
            // The design can change in-case guid won't cut it.
            var uniqueId = Guid.NewGuid().ToString();
            _stateCallbacks[uniqueId] = (pattern, action);
            return uniqueId;
        }

        /// <inheritdoc/>
        public IMediaPlayer MediaPlayer(params string[] entityIds)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.MediaPlayer(this, entityIds);
        }

        /// <inheritdoc/>
        public IMediaPlayer MediaPlayers(IEnumerable<string> entityIds)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.MediaPlayers(this, entityIds);
        }

        /// <inheritdoc/>
        public IMediaPlayer MediaPlayers(Func<IEntityProperties, bool> func)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.MediaPlayers(this, func);
        }

        /// <inheritdoc/>
        public IScript RunScript(params string[] entityIds)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.RunScript(this, entityIds);
        }

        /// <inheritdoc/>
        public Task SaveDataAsync<T>(string id, T data)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.SaveDataAsync<T>(id, data);
        }

        /// <inheritdoc/>
        public async Task<bool> SendEvent(string eventId, dynamic? data = null)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return await _daemon!.SendEvent(eventId, data).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<EntityState?> SetStateAsync(string entityId, dynamic state, params (string name, object val)[] attributes)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return await _daemon!.SetStateAsync(entityId, state, attributes).ConfigureAwait(false);
        }
    }
}