using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    internal enum FluentActionType
    {
        TurnOn,
        TurnOff,
        Toggle,
        SetState
    }

    public interface ITime
    {
        ITimeItems Every(TimeSpan timeSpan);
    }

    public interface ITimeItems
    {
        ITimerEntity Entity(params string[] entityId);
        ITimerEntity Entities(Func<IEntityProperties, bool> func);
    }

    public interface ITimerEntity : ITurnOff<ITimerAction>, ITurnOn<ITimerAction>, IToggle<ITimerAction>
    {
    }

    public interface ITimerAction
    {
        void Execute();

        ITimerAction UsingAttribute(string name, object value);
    }

    public interface IAction : IExecuteAsync
    {
        IAction WithAttribute(string name, object value);
    }

    public interface IEntity : ITurnOff<IAction>, ITurnOn<IAction>, IToggle<IAction>, IStateChanged, ISetState<IAction>
    {
    }

    public interface IEntityProperties
    {
        dynamic? Attribute { get; set; }
        string EntityId { get; set; }

        DateTime LastChanged { get; set; }
        DateTime LastUpdated { get; set; }
        dynamic? State { get; set; }
    }

    public interface IExecuteAsync
    {
        Task ExecuteAsync();
    }

    public interface ILight : ITurnOff<IAction>, ITurnOn<IAction>, IToggle<IAction>, ISetState<IAction>
    {
    }

    public interface IState
    {
        IStateEntity UseEntities(Func<IEntityProperties, bool> func);

        IStateEntity UseEntity(params string[] entityId);
        IStateEntity UseLight(params string[] entity);


        IStateEntity UseLights(Func<IEntityProperties, bool> func);
        IState AndNotChangeFor(TimeSpan timeSpan);
        IExecute Call(Func<string, EntityState?, EntityState?, Task> func);
    }

    public interface IExecute
    {
        void Execute();
    }

    public interface IStateAction : IExecute
    {
        IStateAction WithAttribute(string name, object value);
    }

    public interface IStateChanged
    {
        IState WhenStateChange(object? to = null, object? from = null, bool allChanges = false);
        IState WhenStateChange(Func<EntityState?, EntityState?, bool> stateFunc);
    }

    public interface IStateEntity : ITurnOff<IStateAction>, ITurnOn<IStateAction>, IToggle<IStateAction>,
        ISetState<IStateAction>
    {
    }

    public interface IToggle<T>
    {
        T Toggle();
    }

    public interface ISetState<T>
    {
        T SetState(dynamic state);
    }

    public interface ITurnOff<T>
    {
        T TurnOff();
    }

    public interface ITurnOn<T>
    {
        T TurnOn();
    }

    public class TimeManager : ITime, ITimeItems, ITimerEntity, ITimerAction
    {
        private readonly INetDaemon _daemon;
        private readonly List<string> _entityIds = new List<string>();
        private TimeSpan _timeSpan;
        private EntityManager entityManager;

        public TimeManager(INetDaemon daemon)
        {
            _daemon = daemon;
        }

        public ITimeItems Every(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan;
            return this;
        }

        public ITimerEntity Entity(params string[] entityIds)
        {
            _entityIds.AddRange(entityIds);
            entityManager = new EntityManager(entityIds, _daemon);
            return this;
        }

        public ITimerEntity Entities(Func<IEntityProperties, bool> func)
        {
            var x = _daemon.State.Where(func);
            _entityIds.AddRange(x.Select(n => n.EntityId).ToArray());
            entityManager = new EntityManager(_entityIds.ToArray(), _daemon);
            return this;
        }

        public void Execute()
        {
            _daemon.Scheduler.RunEveryAsync(_timeSpan, async () => { await entityManager.ExecuteAsync(true); });
        }

        public ITimerAction UsingAttribute(string name, object value)
        {
            entityManager.WithAttribute(name, value);
            return this;
        }

        public ITimerAction TurnOff()
        {
            entityManager.TurnOff();
            return this;
        }

        public ITimerAction TurnOn()
        {
            entityManager.TurnOn();
            return this;
        }

        public ITimerAction Toggle()
        {
            entityManager.Toggle();
            return this;
        }
    }

    public class EntityManager : EntityState, IEntity, ILight, IAction, IStateEntity, IState, IStateAction
    {
        private readonly ConcurrentQueue<FluentAction> _actions =
            new ConcurrentQueue<FluentAction>();

        private readonly INetDaemon _daemon;
        private readonly string[] _entityIds;

        private FluentAction? _currentAction;

        private StateChangedInfo _currentState = new StateChangedInfo();


        public EntityManager(string[] entityIds, INetDaemon daemon)
        {
            _entityIds = entityIds;
            _daemon = daemon;
        }

        public async Task ExecuteAsync()
        {
            await ExecuteAsync(false);
        }

        public IAction WithAttribute(string name, object value)
        {
            if (_currentAction != null) _currentAction.Attributes[name] = value;
            return this;
        }

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

        public IState WhenStateChange(Func<EntityState?, EntityState?, bool> stateFunc)
        {
            _currentState = new StateChangedInfo
            {
                Lambda = stateFunc
            };
            return this;
        }

        public IAction Toggle()
        {
            _currentAction = new FluentAction(FluentActionType.Toggle);
            _actions.Enqueue(_currentAction);
            return this;
        }

        public IAction TurnOff()
        {
            _currentAction = new FluentAction(FluentActionType.TurnOff);
            _actions.Enqueue(_currentAction);
            return this;
        }

        public IAction TurnOn()
        {
            _currentAction = new FluentAction(FluentActionType.TurnOn);
            _actions.Enqueue(_currentAction);
            return this;
        }

        IAction ISetState<IAction>.SetState(dynamic state)
        {
            _currentAction = new FluentAction(FluentActionType.SetState);
            _currentAction.State = state;
            _actions.Enqueue(_currentAction);
            return this;
        }

        public IStateEntity UseEntities(Func<IEntityProperties, bool> func)
        {
            _currentState.Entity = _daemon.Entities(func);
            return this;
        }

        public IStateEntity UseEntity(params string[] entityId)
        {
            _currentState.Entity = _daemon.Entity(entityId);
            return this;
        }

        public IStateEntity UseLight(params string[] entity)
        {
            throw new NotImplementedException();
        }

        public IStateEntity UseLights(Func<IEntityProperties, bool> func)
        {
            throw new NotImplementedException();
        }

        public IState AndNotChangeFor(TimeSpan timeSpan)
        {
            _currentState.ForTimeSpan = timeSpan;
            return this;
        }

        public IExecute Call(Func<string, EntityState?, EntityState?, Task> func)
        {
            _currentState.FuncToCall = func;
            return this;
        }

        public void Execute()
        {
            foreach (var entityId in _entityIds)
                _daemon.ListenState(entityId, async (entityIdInn, newState, oldState) =>
                {
                    var entityManager = (EntityManager) _currentState.Entity!;

                    if (_currentState.Lambda != null)
                    {
                        try
                        {
                            if (!_currentState.Lambda(newState, oldState))
                                return;
                        }
                        catch (Exception e)
                        {
                            _daemon.Logger.LogWarning(e, "Failed to evaluate function");
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
                        if (newState?.State == oldState.State && !_currentState.AllChanges)
                            return;
                    }

                    if (_currentState.ForTimeSpan != TimeSpan.Zero)
                    {
                        _daemon.Logger.LogDebug(
                            $"AndNotChangeFor statement found, delaying {_currentState.ForTimeSpan}");
                        await Task.Delay(_currentState.ForTimeSpan);
                        var currentState = _daemon.GetState(entityIdInn);
                        if (currentState != null && currentState.State == newState?.State)
                        {
                            //var timePassed = newState.LastChanged.Subtract(currentState.LastChanged);
                            if (currentState.LastChanged == newState.LastChanged)
                            {
                                // No state has changed during the period
                                _daemon.Logger.LogDebug(
                                    $"State same {newState?.State} during period of {_currentState.ForTimeSpan}, executing action!");
                                // The state has not changed during the time we waited
                                if (_currentState.FuncToCall == null)
                                    await entityManager.ExecuteAsync(true);
                                else
                                    await _currentState.FuncToCall(entityIdInn, newState, oldState);
                            }
                            else
                            {
                                _daemon.Logger.LogDebug(
                                    $"State same {newState?.State} but different state changed: {currentState.LastChanged}, expected {newState.LastChanged}");
                            }
                        }
                        else
                        {
                            _daemon.Logger.LogDebug(
                                $"State not same, do not execute for statement. {newState?.State} found, expected {currentState.State}");
                        }
                    }
                    else
                    {
                        _daemon.Logger.LogDebug(
                            $"State {newState?.State} expected from {oldState?.State}, executing action!");

                        if (_currentState.FuncToCall == null)
                            await entityManager.ExecuteAsync(true);
                        else
                            await _currentState.FuncToCall(entityIdInn, newState, oldState);
                    }
                });
            //}
        }

        IStateAction IStateAction.WithAttribute(string name, object value)
        {
            var entityManager = (EntityManager) _currentState.Entity!;
            entityManager.WithAttribute(name, value);

            return this;
        }

        IStateAction IToggle<IStateAction>.Toggle()
        {
            _currentState?.Entity?.Toggle();
            return this;
        }

        IStateAction ITurnOff<IStateAction>.TurnOff()
        {
            _currentState?.Entity?.TurnOff();
            return this;
        }

        IStateAction ITurnOn<IStateAction>.TurnOn()
        {
            _currentState?.Entity?.TurnOn();
            return this;
        }

        IStateAction ISetState<IStateAction>.SetState(dynamic state)
        {
            _currentState?.Entity?.SetState(state);
            return this;
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
                    await HandleAction(action);
            else
                while (_actions.TryDequeue(out var fluentAction))
                    await HandleAction(fluentAction);
        }

        private async Task HandleAction(FluentAction fluentAction)
        {
            var attributes = fluentAction.Attributes.Select(n => (n.Key, n.Value)).ToArray();
            // Todo: Make it separate tasks ant then await all the same time
            foreach (var entityId in _entityIds)
                switch (fluentAction.ActionType)
                {
                    case FluentActionType.TurnOff:
                        await _daemon.TurnOffAsync(entityId, attributes);
                        break;
                    case FluentActionType.TurnOn:
                        await _daemon.TurnOnAsync(entityId, attributes);
                        break;
                    case FluentActionType.Toggle:
                        await _daemon.ToggleAsync(entityId, attributes);
                        break;
                    case FluentActionType.SetState:
                        await _daemon.SetState(entityId, fluentAction.State, attributes);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
        }
    }

    internal class FluentAction
    {
        public FluentAction(FluentActionType type)
        {
            ActionType = type;
            Attributes = new Dictionary<string, object>();
        }

        public FluentActionType ActionType { get; }
        public Dictionary<string, object> Attributes { get; }
        public dynamic State { get; set; }
    }

    internal class StateChangedInfo
    {
        public IEntity? Entity { get; set; }
        public dynamic? From { get; set; }
        public Func<EntityState?, EntityState?, bool>? Lambda { get; set; }
        public dynamic? To { get; set; }

        public TimeSpan ForTimeSpan { get; set; }
        public Func<string, EntityState?, EntityState?, Task>? FuncToCall { get; set; }
        public bool AllChanges { get; set; }
    }
}