using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.Common.Exceptions;
using NetDaemon.Common.Fluent;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.Common
{
    /// <summary>
    ///     Base class för all NetDaemon apps
    /// </summary>
    [SuppressMessage("", "CA1065"),
     SuppressMessage("", "CA1721")
    ]
    public abstract class NetDaemonApp : NetDaemonAppBase, INetDaemonApp, INetDaemonCommon
    {
        /// <summary>
        ///     All actions being performed for named events
        /// </summary>
        public IList<(string pattern, Func<string, dynamic, Task> action)> EventCallbacks { get; } =
                                            new List<(string pattern, Func<string, dynamic, Task> action)>();

        /// <summary>
        ///     All actions being performed for lambda selected events
        /// </summary>
        public IList<(Func<FluentEventProperty, bool>, Func<string, dynamic, Task>)> EventFunctionCallbacks { get; } = new List<(Func<FluentEventProperty, bool>, Func<string, dynamic, Task>)>();

        /// <inheritdoc/>
        public IScheduler Scheduler => Daemon?.Scheduler ??
            throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
        /// <inheritdoc/>
        public IEnumerable<EntityState> State => Daemon?.State ??
            throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");

        /// <inheritdoc/>
        public ConcurrentDictionary<string, (string pattern, Func<string, EntityState?, EntityState?, Task> action)>
            StateCallbacks
        { get; } = new();

        // Used for testing
        internal ConcurrentDictionary<string, (string pattern, Func<string, EntityState?, EntityState?, Task> action)> InternalStateActions => StateCallbacks;

        /// <inheritdoc/>
        public Task CallService(string domain, string service, dynamic? data = null, bool waitForResponse = false)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return Daemon!.CallServiceAsync(domain, service, data, waitForResponse);
        }

        /// <inheritdoc/>
        public ICamera Camera(params string[] entityIds)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return Daemon!.Camera(this, entityIds);
        }

        /// <inheritdoc/>
        public ICamera Cameras(IEnumerable<string> entityIds)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return Daemon!.Cameras(this, entityIds);
        }

        /// <inheritdoc/>
        public ICamera Cameras(Func<IEntityProperties, bool> func)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return Daemon!.Cameras(this, func);
        }

        /// <inheritdoc/>
        public void CancelListenState(string id)
        {
            // Remove and ignore if not exist
            StateCallbacks.Remove(id, out _);
        }

        /// <inheritdoc/>
        public IDelayResult DelayUntilStateChange(string entityId, object? to = null, object? from = null, bool allChanges = false) =>
            DelayUntilStateChange(new string[] { entityId }, to, from, allChanges);

        /// <inheritdoc/>
        public IDelayResult DelayUntilStateChange(IEnumerable<string> entityIds, object? to = null, object? from = null, bool allChanges = false)
        {
            _ = entityIds ??
                throw new NetDaemonArgumentNullException(nameof(entityIds));

            // Use TaskCompletionSource to simulate a task that we can control
            var taskCompletionSource = new TaskCompletionSource<bool>();
            var result = new DelayResult(taskCompletionSource, this);

            foreach (var entityId in entityIds)
            {
                result.StateSubscriptions.Add(ListenState(entityId, (_, newState, oldState) =>
                {
                    if (to != null && (dynamic)to != newState?.State)
                    {
                        return Task.CompletedTask;
                    }

                    if (from != null && (dynamic)from != oldState?.State)
                    {
                        return Task.CompletedTask;
                    }

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
        [SuppressMessage("", "CA1031")]
        public IDelayResult DelayUntilStateChange(IEnumerable<string> entityIds, Func<EntityState?, EntityState?, bool> stateFunc)
        {
            _ = entityIds ??
                throw new NetDaemonArgumentNullException(nameof(entityIds));

            // Use TaskCompletionSource to simulate a task that we can control
            var taskCompletionSource = new TaskCompletionSource<bool>();
            var result = new DelayResult(taskCompletionSource, this);

            foreach (var entityId in entityIds)
            {
                result.StateSubscriptions.Add(ListenState(entityId, (_, newState, oldState) =>
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
            StateCallbacks.Clear();
            EventCallbacks.Clear();
            EventFunctionCallbacks.Clear();
            await base.DisposeAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public IEntity Entities(Func<IEntityProperties, bool> func)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return Daemon!.Entities(this, func);
        }

        /// <inheritdoc/>
        public IEntity Entities(IEnumerable<string> entityId)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return Daemon!.Entities(this, entityId);
        }

        /// <inheritdoc/>
        public IEntity Entity(params string[] entityId)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return Daemon!.Entity(this, entityId);
        }

        /// <inheritdoc/>
        public IFluentEvent Event(params string[] eventParams) => new FluentEventManager(eventParams, this);

        /// <inheritdoc/>
        public IFluentEvent Events(Func<FluentEventProperty, bool> func) => new FluentEventManager(func, this);

        /// <inheritdoc/>
        public IFluentEvent Events(IEnumerable<string> eventParams) => new FluentEventManager(eventParams, this);

        /// <inheritdoc/>
        public async Task<T?> GetDataAsync<T>(string id) where T : class
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return await Daemon!.GetDataAsync<T>(id).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public EntityState? GetState(string entityId) => Daemon?.GetState(entityId);

        /// <inheritdoc/>
        public IFluentInputSelect InputSelect(params string[] inputSelectParams)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return Daemon!.InputSelect(this, inputSelectParams);
        }

        /// <inheritdoc/>
        public IFluentInputSelect InputSelects(IEnumerable<string> inputSelectParams)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return Daemon!.InputSelects(this, inputSelectParams);
        }

        /// <inheritdoc/>
        public IFluentInputSelect InputSelects(Func<IEntityProperties, bool> func)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return Daemon!.InputSelects(this, func);
        }

        /// <inheritdoc/>
        public void ListenEvent(string ev, Func<string, dynamic, Task> action) => EventCallbacks.Add((ev, action));

        /// <inheritdoc/>
        public void ListenEvent(Func<FluentEventProperty, bool> funcSelector, Func<string, dynamic, Task> action) => EventFunctionCallbacks.Add((funcSelector, action));

        /// <inheritdoc/>
        public string? ListenState(string pattern,
            Func<string, EntityState?, EntityState?, Task> action)
        {
            // Use guid as unique id but will externally use string so
            // The design can change in-case guid won't cut it.
            var uniqueId = Guid.NewGuid().ToString();
            StateCallbacks[uniqueId] = (pattern, action);
            return uniqueId;
        }

        /// <inheritdoc/>
        public IMediaPlayer MediaPlayer(params string[] entityIds)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return Daemon!.MediaPlayer(this, entityIds);
        }

        /// <inheritdoc/>
        public IMediaPlayer MediaPlayers(IEnumerable<string> entityIds)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return Daemon!.MediaPlayers(this, entityIds);
        }

        /// <inheritdoc/>
        public IMediaPlayer MediaPlayers(Func<IEntityProperties, bool> func)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return Daemon!.MediaPlayers(this, func);
        }

        /// <inheritdoc/>
        public IScript RunScript(params string[] entityIds)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return Daemon!.RunScript(this, entityIds);
        }

        /// <inheritdoc/>
        public Task SaveDataAsync<T>(string id, T data)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return Daemon!.SaveDataAsync<T>(id, data);
        }

        /// <inheritdoc/>
        public async Task<bool> SendEvent(string eventId, dynamic? data = null)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return await Daemon!.SendEvent(eventId, data).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<EntityState?> SetStateAsync(string entityId, dynamic state, params (string name, object val)[] attributes)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return await Daemon!.SetStateAsync(entityId, state, attributes).ConfigureAwait(false);
        }
    }
}