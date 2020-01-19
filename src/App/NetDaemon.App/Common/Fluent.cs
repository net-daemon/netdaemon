using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    internal enum FluentActionType
    {
        TurnOn,
        TurnOff,
        WaitFor,

        Toggle
    }

    public interface IAction : IExecuteAsync
    {
        IAction UsingAttribute(string name, object value);
    }

    public interface IEntity : ITurnOff<IAction>, ITurnOn<IAction>, IToggle<IAction>, IStateChanged
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

    public interface ILight : ITurnOff<IAction>, ITurnOn<IAction>, IToggle<IAction>
    {
    }

    public interface IState
    {
        IStateEntity Entities(Func<IEntityProperties, bool> func);

        IStateEntity Entity(params string[] entityId);
        IStateEntity Light(params string[] entity);


        IStateEntity Lights(Func<IEntityProperties, bool> func);
    }

    public interface IStateAction
    {
        void Execute();

        IStateAction UsingAttribute(string name, object value);
    }

    public interface IStateChanged
    {
        IState StateChanged(object? toState, object? from = null);
        IState StateChanged(Func<EntityState?, EntityState?, bool> stateFunc);
    }

    public interface IStateEntity : ITurnOff<IStateAction>, ITurnOn<IStateAction>, IToggle<IStateAction>
    {
    }
    public interface IToggle<T>
    {
        T Toggle();
    }

    public interface ITurnOff<T>
    {
        T TurnOff();
    }

    public interface ITurnOn<T>
    {
        T TurnOn();
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

        public IStateEntity Entities(Func<IEntityProperties, bool> func)
        {
            _currentState.Entity = _daemon.Entities(func);
            return this;
        }

        public IStateEntity Entity(params string[] entityId)
        {
            _currentState.Entity = _daemon.Entity(entityId);
            return this;
        }

        public void Execute()
        {
            foreach (var entityId in _entityIds)
                _daemon.ListenState(entityId, async (entityIdInn, newState, oldState) =>
                {
                    var entityManager = (EntityManager)_currentState.Entity!;

                    if (_currentState.Lambda != null)
                    {
                        if (!_currentState.Lambda(newState, oldState))
                            return;
                    }
                    else
                    {
                        if (_currentState.To != null)
                            if (_currentState.To != newState?.State)
                                return;

                        if (_currentState.From != null)
                            if (_currentState.From != oldState?.State)
                                return;
                    }

                    await entityManager.ExecuteAsync(true);

                });
        }

        public async Task ExecuteAsync()
        {
            await ExecuteAsync(false);
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


            async Task HandleAction(FluentAction fluentAction)
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
                        case FluentActionType.WaitFor:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
            }
        }

        public IStateEntity Light(params string[] entity)
        {
            throw new NotImplementedException();
        }

        public IStateEntity Lights(Func<IEntityProperties, bool> func)
        {
            throw new NotImplementedException();
        }

        public IState StateChanged(object? toState, object? fromState = null)
        {
            _currentState = new StateChangedInfo
            {
                From = fromState,
                To = toState
            };

            return this;
        }

        public IState StateChanged(Func<EntityState?, EntityState?, bool> stateFunc)
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

        IStateAction IToggle<IStateAction>.Toggle()
        {
            _currentState?.Entity?.Toggle();
            return this;
        }

        public IAction TurnOff()
        {
            _currentAction = new FluentAction(FluentActionType.TurnOff);
            _actions.Enqueue(_currentAction);
            return this;
        }

        IStateAction ITurnOff<IStateAction>.TurnOff()
        {
            _currentState?.Entity?.TurnOff();
            return this;
        }

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

        public IAction UsingAttribute(string name, object value)
        {
            if (_currentAction != null) _currentAction.Attributes[name] = value;
            return this;
        }
        IStateAction IStateAction.UsingAttribute(string name, object value)
        {
            var entityManager = (EntityManager)_currentState.Entity!;
            entityManager.UsingAttribute(name, value);

            return this;
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
    }

    internal class StateChangedInfo
    {
        public IEntity? Entity { get; set; }
        public dynamic? From { get; set; }
        public Func<EntityState?, EntityState?, bool>? Lambda { get; set; }
        public dynamic? To { get; set; }
    }
}