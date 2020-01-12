using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace JoySoftware.HomeAssistant.NetDaemon.Common
{

    public interface IAction : ITurnOn, ITurnOff, IToggle, IDelay, IEntityFlow
    {

        //        bool TurnOn(params (string attr, object val)[] attributes);
        //       bool CallService(string service, params (string attr, object val)[] attributes);
    }
    public interface ITurnOn
    {
        IEntity TurnOn { get; }
    }
    public interface ITurnOff
    {
        IEntity TurnOff { get; }
    }

    public interface IToggle
    {
        IEntity Toggle { get; }
    }

    public interface IExecuteAsync
    {
        Task ExecuteAsync();
    }
    public interface IEntityFlow : IEntity
    {
        IEntityFlow UsingAttribute(string name, object value);
    }
    public interface IEntity : IExecuteAsync
    {
        IEntityFlow Entity(string entityId);
        IEntityFlow Entities(params string[] entityIds);

        IAction And { get; }

    }



    public interface IDelay
    {
        Task<IAction> DelayFor(int milliSeconds);
    }

    public static class Extensions
    {


    }

    /// <summary>
    /// Used for extension methods to work internally
    /// </summary>
    internal interface ICareAboutFluent
    {
        INetDaemon Daemon { get; }
        //   string EntityId { get; }
    }

    internal enum ActionType
    {
        TurnOn,
        TurnOff,
        WaitFor,

        Toggle
    }

    internal class FluentEntity
    {
        private FluentEntity(){ }
        public FluentEntity(string entityId) => EntityId = entityId;

        public string EntityId { get; }
        public Dictionary<string, object> Attributes { get; } = new Dictionary<string, object>();

    }
    internal class FluentActionItem
    {
        private FluentActionItem() { }

        internal FluentActionItem(ActionType type) => Type = type;
        public ActionType Type { get; }

        public List<FluentEntity> Entities { get; set; } = new List<FluentEntity>();
    }
    public class FluentAction : IAction, ICareAboutFluent, IEntity
    {
        private readonly INetDaemon _daemon;

        private FluentActionItem _currentAction;

        private readonly ConcurrentQueue<FluentActionItem> _actions = new ConcurrentQueue<FluentActionItem>();

        public FluentAction(INetDaemon daemon)
        {
            _daemon = daemon;
        }

        public async Task<IAction> DelayFor(int milliSeconds)
        {
            Daemon.Logger.LogInformation("First DelayFor 1000");
            await Task.Delay(milliSeconds);
            return this;
        }

        public INetDaemon Daemon => _daemon;

        public IEntity TurnOn
        {
            get
            {
                _currentAction = new FluentActionItem(ActionType.TurnOn);
                _actions.Enqueue(_currentAction);
                return this;
            }
        }

        public IEntity TurnOff
        {
            get
            {
                _currentAction = new FluentActionItem(ActionType.TurnOff);
                _actions.Enqueue(_currentAction);
                return this;
            }
        }

        public IEntity Toggle
        {
            get
            {
                _currentAction = new FluentActionItem(ActionType.Toggle);
                _actions.Enqueue(_currentAction);
                return this;
            }
        }
        
        public IEntityFlow Entity(string entityId)
        {
            _currentAction.Entities.Add(new FluentEntity(entityId));
            return this;
        }
        public IEntityFlow Entities(params string[] entityIds)
        {
            _currentAction.Entities.AddRange(entityIds.Select(n=>new FluentEntity(n)));
            return this;
        }

        IAction IEntity.And => this;
        public IEntityFlow UsingAttribute(string name, object value)
        {
            // Make sure all entities belonging to the entity is using the attribute
            foreach (var entity in _currentAction.Entities)
            {
                entity.Attributes[name] = value;
            }

            return this;
        }

        public IAction Action() => throw new NotImplementedException();

        public async Task ExecuteAsync()
        {
            FluentActionItem action;
            while (_actions.TryDequeue(out action))
            {
                switch (action.Type)
                {
                    case ActionType.TurnOff:
                        foreach (var entity in action.Entities)
                        {
                            var attributes = entity.Attributes.Select(n => (n.Key, n.Value)).ToArray();
                            // Todo: optimize later for more parallel 
                            await _daemon.TurnOffAsync(entity.EntityId, attributes);
                        }
                        break;

                    case ActionType.TurnOn:
                        foreach (var entity in action.Entities)
                        {
                            var attributes = entity.Attributes.Select(n => (n.Key, n.Value)).ToArray();
                            // Todo: optimize later for more parallel 
                            await _daemon.TurnOnAsync(entity.EntityId, attributes);
                        }

                        break;
                    case ActionType.Toggle:
                        foreach (var entity in action.Entities)
                        {
                            var attributes = entity.Attributes.Select(n => (n.Key, n.Value)).ToArray();
                            // Todo: optimize later for more parallel 
                            await _daemon.ToggleAsync(entity.EntityId, attributes);
                        }

                        break;
                }
            }
        }

    }

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
