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
    ///     A class that implements the management of delays and cancel them
    /// </summary>
    public class DelayResult : IDelayResult
    {
        private readonly INetDaemonApp _daemon;
        private readonly TaskCompletionSource<bool> _delayTaskCompletionSource;
        private bool _isCanceled = false;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="delayTaskCompletionSource"></param>
        /// <param name="daemon"></param>
        public DelayResult(TaskCompletionSource<bool> delayTaskCompletionSource, INetDaemonApp daemon)
        {
            _delayTaskCompletionSource = delayTaskCompletionSource;
            _daemon = daemon;
        }

        /// <inheritdoc/>
        public Task<bool> Task => _delayTaskCompletionSource.Task;

        internal ConcurrentBag<string> StateSubscriptions { get; set; } = new ConcurrentBag<string>();

        /// <inheritdoc/>
        public void Cancel()
        {
            if (_isCanceled)
                return;

            _isCanceled = true;
            foreach (var stateSubscription in StateSubscriptions)
            {
                //Todo: Handle
                _daemon.CancelListenState(stateSubscription);
            }
            StateSubscriptions.Clear();

            // Also cancel all await if this is disposed
            _delayTaskCompletionSource.TrySetResult(false);
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Make sure any subscriptions are canceled
                    Cancel();
                }
                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }

    /// <summary>
    ///     Base class för all NetDaemon apps
    /// </summary>
    public abstract class NetDaemonApp : NetDaemonAppBase, INetDaemonApp, INetDaemonCommon
    {
        private readonly IList<(string pattern, Func<string, dynamic, Task> action)> _eventActions =
                                    new List<(string pattern, Func<string, dynamic, Task> action)>();

        private readonly List<(Func<FluentEventProperty, bool>, Func<string, dynamic, Task>)> _eventFunctionList =
                    new List<(Func<FluentEventProperty, bool>, Func<string, dynamic, Task>)>();

        private readonly List<(string, string, Func<dynamic?, Task>)> _serviceCallFunctionList
            = new List<(string, string, Func<dynamic?, Task>)>();

        private readonly ConcurrentDictionary<string, (string pattern, Func<string, EntityState?, EntityState?, Task> action)> _stateActions =
                                    new ConcurrentDictionary<string, (string pattern, Func<string, EntityState?, EntityState?, Task> action)>();
        /// <summary>
        ///     All actions being performed for named events
        /// </summary>
        public IList<(string pattern, Func<string, dynamic, Task> action)> EventActions => _eventActions;
        /// <summary>
        ///     All actions being performed for lambda selected events
        /// </summary>
        public List<(Func<FluentEventProperty, bool>, Func<string, dynamic, Task>)> EventFunctions => _eventFunctionList;

        /// <inheritdoc/>
        public IScheduler Scheduler => _daemon?.Scheduler ??
            throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

        public List<(string, string, Func<dynamic?, Task>)> ServiceCallFunctions => _serviceCallFunctionList;

        /// <inheritdoc/>
        public IEnumerable<EntityState> State => _daemon?.State ??
            throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

        /// <inheritdoc/>
        public ConcurrentDictionary<string, (string pattern, Func<string, EntityState?, EntityState?, Task> action)>
            StateActions => _stateActions;

        // Used for testing
        internal ConcurrentDictionary<string, (string pattern, Func<string, EntityState?, EntityState?, Task> action)> InternalStateActions => _stateActions;
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
        public void CancelListenState(string id)
        {
            // Remove and ignore if not exist
            _stateActions.Remove(id, out _);
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

            //Todo: FIX
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
        /// <returns></returns>
        public async override ValueTask DisposeAsync()
        {
            _stateActions.Clear();
            _eventActions.Clear();
            _eventFunctionList.Clear();
            _serviceCallFunctionList.Clear();

            await base.DisposeAsync().ConfigureAwait(false);
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
        public IFluentEvent Event(params string[] eventParams) => new FluentEventManager(eventParams, this);

        /// <inheritdoc/>
        public IFluentEvent Events(Func<FluentEventProperty, bool> func) => new FluentEventManager(func, this);

        /// <inheritdoc/>
        public IFluentEvent Events(IEnumerable<string> eventParams) => new FluentEventManager(eventParams, this);

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
        public void ListenEvent(string ev, Func<string, dynamic, Task> action) => _eventActions.Add((ev, action));

        /// <inheritdoc/>
        public void ListenEvent(Func<FluentEventProperty, bool> funcSelector, Func<string, dynamic, Task> func) => _eventFunctionList.Add((funcSelector, func));

        /// <inheritdoc/>
        public void ListenServiceCall(string domain, string service, Func<dynamic?, Task> action)
            => _serviceCallFunctionList.Add((domain.ToLowerInvariant(), service.ToLowerInvariant(), action));
        /// <inheritdoc/>
        public string? ListenState(string pattern,
            Func<string, EntityState?, EntityState?, Task> action)
        {
            // Use guid as uniqe id but will externally use string so
            // The design can change incase guild wont cut it
            var uniqueId = Guid.NewGuid().ToString();
            _stateActions[uniqueId] = (pattern, action);
            return uniqueId.ToString();
        }
        // /// <inheritdoc/>
        // public void ListenEvent(string ev, Func<string, dynamic?, Task> action) => _daemon?.ListenEvent(ev, action);

        // /// <inheritdoc/>
        // public void ListenEvent(Func<FluentEventProperty, bool> funcSelector, Func<string, dynamic, Task> func) =>
        //         _daemon?.ListenEvent(funcSelector, func);

        // /// <inheritdoc/>
        // public void ListenServiceCall(string domain, string service, Func<dynamic?, Task> action) =>
        //         _daemon?.ListenServiceCall(domain, service, action);

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
    }
}