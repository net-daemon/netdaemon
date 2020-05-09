using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    /// <summary>
    ///     Implements interface for managing entities in the fluent API
    /// </summary>
    public class EntityManager : EntityBase, IEntity, IAction,
        IStateEntity, IState, IStateAction, IScript, IDelayStateChange
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="entityIds">The unique ids of the entities managed</param>
        /// <param name="daemon">The Daemon that will handle API calls to Home Assistant</param>
        /// <param name="app">The Daemon App calling fluent API</param>
        public EntityManager(IEnumerable<string> entityIds, INetDaemon daemon, INetDaemonApp app) : base(entityIds, daemon, app)
        {
        }

        /// <inheritdoc/>
        public IState AndNotChangeFor(TimeSpan timeSpan)
        {
            _currentState.ForTimeSpan = timeSpan;
            return this;
        }

        /// <inheritdoc/>
        public IExecute Call(Func<string, EntityState?, EntityState?, Task> func)
        {
            _currentState.FuncToCall = func;
            return this;
        }

        /// <inheritdoc/>
        IDelayResult IDelayStateChange.DelayUntilStateChange(object? to, object? from, bool allChanges)
        {
            return this.Daemon.DelayUntilStateChange(this.EntityIds, to, from, allChanges);
        }

        /// <inheritdoc/>
        IDelayResult IDelayStateChange.DelayUntilStateChange(Func<EntityState?, EntityState?, bool> stateFunc)
        {
            return this.Daemon.DelayUntilStateChange(this.EntityIds, stateFunc);
        }

        /// <inheritdoc/>
        public void Execute()
        {
            foreach (var entityId in EntityIds)
                Daemon.ListenState(entityId, async (entityIdInn, newState, oldState) =>
                {
                    try
                    {
                        var entityManager = (EntityManager)_currentState.Entity!;

                        if (_currentState.Lambda != null)
                        {
                            try
                            {
                                if (!_currentState.Lambda(newState, oldState))
                                    return;
                            }
                            catch (Exception e)
                            {
                                Daemon.Logger.LogWarning(e,
                                    "Failed to evaluate function in App {appId}, EntityId: {entityId}, From: {newState} To: {oldState}", App.Id, entityIdInn, $"{newState?.State}", $"{oldState?.State}");
                                return;
                            }
                        }
                        else
                        {
                            if (_currentState.To != null)
                                if (_currentState.To != newState?.State)
                                    return;

                            if (_currentState.From != null)
                                if (_currentState.From != oldState?.State)
                                    return;

                            // If we don´t accept all changes in the state change
                            // and we do not have a state change so return
                            if (newState?.State == oldState?.State && !_currentState.AllChanges)
                                return;
                        }

                        if (_currentState.ForTimeSpan != TimeSpan.Zero)
                        {
                            Daemon.Logger.LogDebug(
                                "AndNotChangeFor statement found, delaying {time}", _currentState.ForTimeSpan);
                            await Task.Delay(_currentState.ForTimeSpan).ConfigureAwait(false);
                            var currentState = Daemon.GetState(entityIdInn);
                            if (currentState != null && currentState.State == newState?.State)
                            {
                                //var timePassed = newState.LastChanged.Subtract(currentState.LastChanged);
                                if (currentState?.LastChanged == newState?.LastChanged)
                                {
                                    // No state has changed during the period
                                    Daemon.Logger.LogDebug(
                                        "State same {newState} during period of {time}, executing action!", $"{newState?.State}", _currentState.ForTimeSpan);
                                    // The state has not changed during the time we waited
                                    if (_currentState.FuncToCall == null)
                                        await entityManager.ExecuteAsync(true).ConfigureAwait(false);
                                    else
                                    {
                                        try
                                        {
                                            await _currentState.FuncToCall(entityIdInn, newState, oldState).ConfigureAwait(false);
                                        }
                                        catch (Exception e)
                                        {
                                            Daemon.Logger.LogWarning(e,
                                                "Call function error in timespan in App {appId}, EntityId: {entityId}, From: {newState} To: {oldState}",
                                                    App.Id, entityIdInn, $"{newState?.State}", $"{oldState?.State}");
                                        }
                                    }
                                }
                                else
                                {
                                    Daemon.Logger.LogDebug(
                                        "State same {newState} but different state changed: {currentLastChanged}, expected {newLastChanged}",
                                            $"{newState?.State}",
                                            currentState?.LastChanged,
                                            newState?.LastChanged);
                                }
                            }
                            else
                            {
                                Daemon.Logger.LogDebug(
                                    "State not same, do not execute for statement. {newState} found, expected {currentState}",
                                    $"{newState?.State}",
                                    $"{currentState?.State}");
                            }
                        }
                        else
                        {
                            Daemon.Logger.LogDebug(
                                "State {newState} expected from {oldState}, executing action!",
                                    $"{newState?.State}",
                                    $"{oldState?.State}"
                                    );

                            if (_currentState.FuncToCall != null)
                            {
                                try
                                {
                                    await _currentState.FuncToCall(entityIdInn, newState, oldState).ConfigureAwait(false);
                                }
                                catch (Exception e)
                                {
                                    Daemon.Logger.LogWarning(e,
                                               "Call function error in App {appId}, EntityId: {entityId}, From: {newState} To: {oldState}",
                                                   App.Id, entityIdInn, $"{newState?.State}", $"{oldState?.State}");
                                }
                            }
                            else if (_currentState.ScriptToCall != null)
                                await Daemon.RunScript(App, _currentState.ScriptToCall).ExecuteAsync().ConfigureAwait(false);
                            else
                                await entityManager.ExecuteAsync(true).ConfigureAwait(false);
                        }
                    }
                    catch (Exception e)
                    {
                        Daemon.Logger.LogWarning(e, "Unhandled error in ListenState in App {appId}", App.Id);
                    }
                });

            //}
        }

        /// <inheritdoc/>
        Task IExecuteAsync.ExecuteAsync()
        {
            return ExecuteAsync();
        }

        /// <inheritdoc/>
        async Task IScript.ExecuteAsync()
        {
            var taskList = new List<Task>();
            foreach (var scriptName in EntityIds)
            {
                var name = scriptName;
                if (scriptName.Contains('.'))
                    name = scriptName[(scriptName.IndexOf('.') + 1)..];
                var task = Daemon.CallServiceAsync("script", name);
                taskList.Add(task);
            }

            // Wait for all tasks to complete or max 5 seconds
            if (taskList.Count > 0) await Task.WhenAny(Task.WhenAll(taskList.ToArray()), Task.Delay(5000)).ConfigureAwait(false);
        }

        /// <summary>
        ///     Executes the sequence of actions
        /// </summary>
        /// <param name="keepItems">
        ///     True if  you want to keep items
        /// </param>
        /// <remarks>
        ///     You want to keep the items when using this as part of an automation
        ///     that are kept over time. Not keeping when just doing a command
        /// </remarks>
        /// <returns></returns>
        public async Task ExecuteAsync(bool keepItems)
        {
            if (keepItems)
                foreach (var action in _actions)
                    await HandleAction(action).ConfigureAwait(false);
            else
                while (_actions.TryDequeue(out var fluentAction))
                    await HandleAction(fluentAction).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public IExecute RunScript(params string[] entityIds)
        {
            _currentState.ScriptToCall = entityIds;
            return this;
        }

        /// <inheritdoc/>
        IAction ISetState<IAction>.SetState(dynamic state)
        {
            _currentAction = new FluentAction(FluentActionType.SetState);
            _currentAction.State = state;
            _actions.Enqueue(_currentAction);
            return this;
        }

        /// <inheritdoc/>
        IStateAction ISetState<IStateAction>.SetState(dynamic state)
        {
            _currentState?.Entity?.SetState(state);
            return this;
        }

        /// <inheritdoc/>
        public IAction Toggle()
        {
            _currentAction = new FluentAction(FluentActionType.Toggle);
            _actions.Enqueue(_currentAction);
            return this;
        }

        /// <inheritdoc/>
        IStateAction IToggle<IStateAction>.Toggle()
        {
            _currentState?.Entity?.Toggle();
            return this;
        }

        /// <inheritdoc/>
        public IAction TurnOff()
        {
            _currentAction = new FluentAction(FluentActionType.TurnOff);
            _actions.Enqueue(_currentAction);
            return this;
        }

        /// <inheritdoc/>
        IStateAction ITurnOff<IStateAction>.TurnOff()
        {
            _currentState?.Entity?.TurnOff();
            return this;
        }

        /// <inheritdoc/>
        public IAction TurnOn()
        {
            _currentAction = new FluentAction(FluentActionType.TurnOn);
            _actions.Enqueue(_currentAction);
            return this;
        }

        IStateAction ITurnOn<IStateAction>.TurnOn()
        {
            _currentState?.Entity?.TurnOn();
            return this;
        }

        /// <inheritdoc/>
        public IStateEntity UseEntities(Func<IEntityProperties, bool> func)
        {
            _currentState.Entity = Daemon.Entities(App, func);
            return this;
        }

        /// <inheritdoc/>
        public IStateEntity UseEntities(IEnumerable<string> entities)
        {
            _currentState.Entity = Daemon.Entities(App, entities);
            return this;
        }

        /// <inheritdoc/>
        public IStateEntity UseEntity(params string[] entityId)
        {
            _currentState.Entity = Daemon.Entity(App, entityId);
            return this;
        }

        /// <inheritdoc/>
        public IState WhenStateChange(object? to = null, object? from = null, bool allChanges = false)
        {
            _currentState = new StateChangedInfo
            {
                From = from,
                To = to,
                AllChanges = allChanges
            };

            return this;
        }

        /// <inheritdoc/>
        public IState WhenStateChange(Func<EntityState?, EntityState?, bool> stateFunc)
        {
            _currentState = new StateChangedInfo
            {
                Lambda = stateFunc
            };
            return this;
        }

        /// <inheritdoc/>
        public IAction WithAttribute(string name, object value)
        {
            if (_currentAction != null) _currentAction.Attributes[name] = value;
            return this;
        }

        /// <inheritdoc/>
        IStateAction IStateAction.WithAttribute(string name, object value)
        {
            var entityManager = (EntityManager)_currentState.Entity!;
            entityManager.WithAttribute(name, value);

            return this;
        }

        /// <inheritdoc/>
        private Task ExecuteAsync()
        {
            return ExecuteAsync(false);
        }

        /// <inheritdoc/>
        private async Task HandleAction(FluentAction fluentAction)
        {
            var attributes = fluentAction.Attributes.Select(n => (n.Key, n.Value)).ToArray();

            var taskList = new List<Task>();
            foreach (var entityId in EntityIds)
            {
                var task = fluentAction.ActionType switch
                {
                    FluentActionType.TurnOff => TurnOffAsync(entityId, attributes),
                    FluentActionType.TurnOn => TurnOnAsync(entityId, attributes),
                    FluentActionType.Toggle => ToggleAsync(entityId, attributes),
                    FluentActionType.SetState => Daemon.SetStateAsync(entityId, fluentAction.State, attributes),
                    _ => throw new NotSupportedException($"Fluent action type not handled! {fluentAction.ActionType}")
                };
                taskList.Add(task);
            }
            // Wait for all tasks to complete or max 5 seconds
            if (taskList.Count > 0) await Task.WhenAny(Task.WhenAll(taskList.ToArray()), Task.Delay(5000)).ConfigureAwait(false);
        }

        private Task ToggleAsync(string entityId, params (string name, object val)[] attributeNameValuePair)
        {
            // Get the domain if supported, else domain is homeassistant
            string domain = GetDomainFromEntity(entityId);
            // Use it if it is supported else use default "homeassistant" domain

            // Use expando object as all other methods
            dynamic attributes = attributeNameValuePair.ToDynamic();
            // and add the entity id dynamically
            attributes.entity_id = entityId;

            return Daemon.CallServiceAsync(domain, "toggle", attributes, false);
        }

        private Task TurnOffAsync(string entityId, params (string name, object val)[] attributeNameValuePair)
        {
            // Get the domain if supported, else domain is homeassistant
            string domain = GetDomainFromEntity(entityId);
            // Use it if it is supported else use default "homeassistant" domain

            // Use expando object as all other methods
            dynamic attributes = attributeNameValuePair.ToDynamic();
            // and add the entity id dynamically
            attributes.entity_id = entityId;

            return Daemon.CallServiceAsync(domain, "turn_off", attributes, false);
        }

        private Task TurnOnAsync(string entityId, params (string name, object val)[] attributeNameValuePair)
        {
            // Use default domain "homeassistant" if supported is missing
            string domain = GetDomainFromEntity(entityId);
            // Use it if it is supported else use default "homeassistant" domain

            // Convert the value pairs to dynamic type
            dynamic attributes = attributeNameValuePair.ToDynamic();
            // and add the entity id dynamically
            attributes.entity_id = entityId;

            return Daemon.CallServiceAsync(domain, "turn_on", attributes, false);
        }
    }
}