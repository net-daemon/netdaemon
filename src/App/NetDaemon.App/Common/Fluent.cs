using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;


namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    public interface IExecuteAsync
    {
        Task ExecuteAsync();
    }

    public interface ITurnOff
    {
        IAction TurnOff();
    }

    public interface ITurnOn
    {
        IAction TurnOn();
    }

    public interface IToggle
    {
        IAction Toggle();
    }

    public interface IAction : IExecuteAsync
    {
        IAction UsingAttribute(string name, object value);
    }

    public interface IEntityProperties
    {
        string EntityId { get; set; }

        string State { get; set; }

        dynamic Attribute { get; set; }

        DateTime LastChanged { get; set; }

        DateTime LastUpdated { get; set; }
    }
    public interface IEntity : ITurnOff, ITurnOn, IToggle
    {

    }

    public interface ILight : ITurnOff, ITurnOn, IToggle
    {

    }

    internal enum FluentActionType
    {
        TurnOn,
        TurnOff,
        WaitFor,

        Toggle
    }

    internal class FluentAction
    {
        public Dictionary<string, object> Attributes { get; }

        public FluentActionType ActionType { get; }
        public FluentAction(FluentActionType type)
        {
            ActionType = type;
            Attributes = new Dictionary<string, object>();
        }

    }
    public class EntityManager : EntityState, IEntity, ILight, IAction, IEntityProperties
    {
        private string[] _entityIds;
        private readonly INetDaemon _daemon;
        private readonly ConcurrentQueue<FluentAction> _actions = new ConcurrentQueue<FluentAction>();
        
        private FluentAction _currentAction;

        public EntityManager(string[] entityIds, INetDaemon daemon)
        {
            _entityIds = entityIds;
            _daemon = daemon;
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

        public IAction Toggle()
        {
            _currentAction = new FluentAction(FluentActionType.Toggle);
            _actions.Enqueue(_currentAction);
            return this;
        }

        public IAction UsingAttribute(string name, object value)
        {
            _currentAction.Attributes[name] = value;
            return this;
        }

        public async Task ExecuteAsync()
        {
            while (_actions.TryDequeue(out var fluentAction))
            {
                var attributes = fluentAction.Attributes.Select(n => (n.Key, n.Value)).ToArray();
                // Todo: Make it separate tasks ant then await all the same time
                foreach (var entityId in _entityIds)
                {
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
                    }

                }
            }
            
        }

        
    }
    //public interface IAction : ITurnOn, ITurnOff, IToggle, IDelay, IEntityFlow
    //{

    //    //        bool TurnOn(params (string attr, object val)[] attributes);
    //    //       bool CallService(string service, params (string attr, object val)[] attributes);
    //}
    //public interface ITurnOn
    //{
    //    IEntity TurnOn { get; }
    //}
    //public interface ITurnOff
    //{
    //    IEntity TurnOff { get; }
    //}

    //public interface IToggle
    //{
    //    IEntity Toggle { get; }
    //}

    //public interface IExecuteAsync
    //{
    //    Task ExecuteAsync();
    //}
    //public interface IEntityFlow : IEntity
    //{
    //    IEntityFlow UsingAttribute(string name, object value);
    //    IEntity Where(Func<EntityState, bool> func);
    //}
    //public interface IEntity : IExecuteAsync
    //{
    //    IEntityFlow Entity(string entityId);
    //    IEntityFlow Entities(params string[] entityIds);

    //    IAction And { get; }

    //    //IEntityFlow Entity(Func<string, bool> entityId);
    //}



    //public interface IDelay
    //{
    //    Task<IAction> DelayFor(int milliSeconds);
    //}

    //public static class Extensions
    //{


    //}

    ///// <summary>
    ///// Used for extension methods to work internally
    ///// </summary>
    //internal interface ICareAboutFluent
    //{
    //    INetDaemon Daemon { get; }
    //    //   string EntityId { get; }
    //}

    //internal enum ActionType
    //{
    //    TurnOn,
    //    TurnOff,
    //    WaitFor,

    //    Toggle
    //}

    //internal class FluentEntity
    //{
    //    public FluentEntity()
    //    {
    //        EntityIds = new List<string>();
    //        Attributes = new Dictionary<string, object>();
    //    }

    //    public FluentEntity(string entityId, IDictionary<string, object>? attributes = null)
    //    {
    //        EntityIds = new List<string> { entityId };
    //        Attributes = attributes ?? new Dictionary<string, object>();
    //    }

    //    public FluentEntity(IEnumerable<string> entityIds, IDictionary<string, object>? attributes = null)
    //    {
    //        EntityIds = new List<string>();
    //        EntityIds.AddRange(entityIds);
    //        Attributes = attributes ?? new Dictionary<string, object>();
    //    }
    //    public List<string> EntityIds { get; }
    //    public IDictionary<string, object> Attributes { get; }

    //}
    //internal class FluentActionItem
    //{
    //    private FluentActionItem() { }

    //    internal FluentActionItem(ActionType type) => Type = type;
    //    public ActionType Type { get; }

    //    public List<FluentEntity> Entity { get; set; } = new List<FluentEntity>();
    //    // public Func<EntityState, bool> WhereSelection { get; set; }
    //}
    //public class FluentAction : IAction, ICareAboutFluent, IEntity
    //{
    //    private readonly INetDaemon _daemon;

    //    private FluentActionItem _currentAction;
    //    private FluentEntity _currentEntity;

    //    private readonly ConcurrentQueue<FluentActionItem> _actions = new ConcurrentQueue<FluentActionItem>();

    //    public FluentAction(INetDaemon daemon)
    //    {
    //        _daemon = daemon;
    //    }

    //    public async Task<IAction> DelayFor(int milliSeconds)
    //    {
    //        Daemon.Logger.LogInformation("First DelayFor 1000");
    //        await Task.Delay(milliSeconds);
    //        return this;
    //    }

    //    public INetDaemon Daemon => _daemon;

    //    public IEntity TurnOn
    //    {
    //        get
    //        {
    //            _currentAction = new FluentActionItem(ActionType.TurnOn);
    //            _actions.Enqueue(_currentAction);
    //            return this;
    //        }
    //    }

    //    public IEntity TurnOff
    //    {
    //        get
    //        {
    //            _currentAction = new FluentActionItem(ActionType.TurnOff);
    //            _actions.Enqueue(_currentAction);
    //            return this;
    //        }
    //    }

    //    public IEntity Toggle
    //    {
    //        get
    //        {
    //            _currentAction = new FluentActionItem(ActionType.Toggle);
    //            _actions.Enqueue(_currentAction);
    //            return this;
    //        }
    //    }

    //    public IEntityFlow Entity(string entityId)
    //    {
    //        _currentEntity = new FluentEntity(entityId);
    //        _currentAction.Entity.Add(_currentEntity);
    //        return this;
    //    }
    //    public IEntityFlow Entities(params string[] entityIds)
    //    {
    //        _currentEntity = new FluentEntity(entityIds);
    //        _currentAction.Entity.Add(_currentEntity);
    //        return this;
    //    }

    //    IAction IEntity.And => this;
    //    public IEntityFlow UsingAttribute(string name, object value)
    //    {
    //        _currentEntity.Attributes[name] = value;
    //        return this;
    //    }

    //    public IEntity Where(Func<EntityState, bool> func)
    //    {
    //        var currentStates = _daemon.State;

    //        if (_currentEntity.EntityIds.Count > 0)
    //        {

    //            currentStates = currentStates.Where(n =>
    //                _currentEntity.EntityIds.Contains(n.EntityId)).ToList();
    //        }
    //        var filteredEntityIds = currentStates.Where(func).Select(n => n.EntityId).ToList();
    //        _currentEntity.EntityIds.RemoveAll(n => filteredEntityIds.Contains(n) == false);
    //        return this;
    //    }

    //    public IAction Action() => throw new NotImplementedException();

    //    public async Task ExecuteAsync()
    //    {
    //        FluentActionItem action;
    //        while (_actions.TryDequeue(out action))
    //        {
    //            switch (action.Type)
    //            {
    //                case ActionType.TurnOff:
    //                    foreach (var fluentEntity in action.Entity)
    //                    {
    //                        foreach (var entity in fluentEntity.EntityIds)
    //                        {
    //                            var attributes = fluentEntity.Attributes.Select(n => (n.Key, n.Value)).ToArray();
    //                            // Todo: optimize later for more parallel 
    //                            await _daemon.TurnOffAsync(entity, attributes);
    //                        }

    //                    }
    //                    break;

    //                case ActionType.TurnOn:
    //                    foreach (var fluentEntity in action.Entity)
    //                    {
    //                        foreach (var entity in fluentEntity.EntityIds)
    //                        {
    //                            var attributes = fluentEntity.Attributes.Select(n => (n.Key, n.Value)).ToArray();
    //                            // Todo: optimize later for more parallel 
    //                            await _daemon.TurnOnAsync(entity, attributes);
    //                        }

    //                    }

    //                    break;
    //                case ActionType.Toggle:
    //                    foreach (var fluentEntity in action.Entity)
    //                    {
    //                        foreach (var entity in fluentEntity.EntityIds)
    //                        {
    //                            var attributes = fluentEntity.Attributes.Select(n => (n.Key, n.Value)).ToArray();
    //                            // Todo: optimize later for more parallel 
    //                            await _daemon.ToggleAsync(entity, attributes);
    //                        }


    //                    }

    //                    break;
    //            }
    //        }
    //    }

    //}

    //public static Task<IAction> DelayFor(this Task<IAction> previousStep, int milliSeconds)
    //{
    //    return previousStep.ContinueWith(async (task) =>
    //    {
    //        var entity = await previousStep;
    //        await Task.Delay(milliSeconds);
    //        return task.Result;
    //    }).Unwrap();
    //}

    //public static Task<IAction> TurnOn(this Task<IAction> previousStep, params (string attr, object val)[] attributes)
    //{
    //    return previousStep.ContinueWith(async (task) =>
    //    {
    //        var entity = await previousStep;
    //        var fluent = (ICareAboutFluent) entity;
    //        await fluent.Daemon.TurnOnAsync(fluent.EntityId, attributes);
    //        return task.Result;
    //    }).Unwrap();
    //}

    //public static Task<IAction> TurnOff(this Task<IAction> previousStep)
    //{
    //    return previousStep.ContinueWith(async (task) =>
    //    {
    //        var entity = await previousStep;
    //        var fluent = entity as ICareAboutFluent;
    //        await fluent.Daemon.TurnOffAsync(fluent.EntityId);
    //        return task.Result;
    //    }).Unwrap();

    //}
}
