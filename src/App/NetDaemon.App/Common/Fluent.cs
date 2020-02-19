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
        SetState,
        Play,
        Pause,
        PlayPause,
        Stop,
        Speak
    }

    /// <summary>
    ///     Actions to execute on entity
    /// </summary>
    public interface IAction : IExecuteAsync
    {

        /// <summary>
        ///     Use attribute when perform action
        /// </summary>
        /// <param name="name">The name of attribute</param>
        /// <param name="value">The value of attribute</param>
        IAction WithAttribute(string name, object value);
    }

    /// <summary>
    ///     Represents an entity
    /// </summary>
    public interface IEntity :
        ITurnOff<IAction>, ITurnOn<IAction>, IToggle<IAction>,
        IStateChanged, ISetState<IAction>
    { }

    /// <summary>
    ///     Properties on entities that can be filtered in lambda expression
    /// </summary>
    public interface IEntityProperties
    {
        /// <summary>
        ///     Filter on attribute
        /// </summary>
        dynamic? Attribute { get; set; }

        /// <summary>
        ///     Filter on unique id of the entity
        /// </summary>
        string EntityId { get; set; }

        /// <summary>
        ///     Filter on last changed time
        /// </summary>
        DateTime LastChanged { get; set; }

        /// <summary>
        ///     Filter on last updated time
        /// </summary>
        DateTime LastUpdated { get; set; }

        /// <summary>
        ///     Filter on state
        /// </summary>
        dynamic? State { get; set; }
    }

    /// <summary>
    ///     Expressions that ends with execute
    /// </summary>
    public interface IExecute
    {
        /// <summary>
        ///     Executes the expression
        /// </summary>
        void Execute();
    }

    /// <summary>
    ///     Expressions that ends with async excecution
    /// </summary>
    public interface IExecuteAsync
    {
        /// <summary>
        ///     Execute action async
        /// </summary>
        Task ExecuteAsync();
    }

    /// <summary>
    ///     Light entity
    /// </summary>
    public interface ILight : ITurnOff<IAction>, ITurnOn<IAction>, IToggle<IAction>, ISetState<IAction>
    {
    }

    /// <summary>
    ///     Represents media player actions
    /// </summary>
    public interface IMediaPlayer : IPlay<IMediaPlayerExecuteAsync>,
        IStop<IMediaPlayerExecuteAsync>, IPlayPause<IMediaPlayerExecuteAsync>,
        IPause<IMediaPlayerExecuteAsync>, ISpeak<IMediaPlayerExecuteAsync>
    {
    }

    /// <summary>
    ///     Excecutes media player actions async
    /// </summary>
    public interface IMediaPlayerExecuteAsync
    {
        /// <summary>
        ///     Excecutes media player actions async
        /// </summary>
        Task ExecuteAsync();
    }

    /// <summary>
    ///     Represents a script entity
    /// </summary>
    public interface IScript
    {
        /// <summary>
        ///     Excecutes scripts async
        /// </summary>
        Task ExecuteAsync();
    }

    /// <summary>
    ///     Represent state change actions
    /// </summary>
    public interface IState
    {
        /// <summary>
        ///     The state has not changed for a period of time
        /// </summary>
        /// <param name="timeSpan">Period of time state should not change</param>
        IState AndNotChangeFor(TimeSpan timeSpan);

        /// <summary>
        ///     Call a callback function or func expression
        /// </summary>
        /// <param name="func">The action to call</param>
        IExecute Call(Func<string, EntityState?, EntityState?, Task> func);

        /// <summary>
        ///     Run script
        /// </summary>
        /// <param name="entityIds">Ids of the scripts that should be run</param>
        IExecute RunScript(params string[] entityIds);

        /// <summary>
        ///     Use entities with lambda expression for further actions
        /// </summary>
        /// <param name="func">Lambda expression to filter out entities</param>
        IStateEntity UseEntities(Func<IEntityProperties, bool> func);

        /// <summary>
        ///     Use entities from list
        /// </summary>
        /// <param name="entities">The entities to perform actions on</param>
        IStateEntity UseEntities(IEnumerable<string> entities);

        /// <summary>
        ///     Use entity or multiple entities
        /// </summary>
        /// <param name="entityId">Unique id of the entity provided</param>
        IStateEntity UseEntity(params string[] entityId);
    }

    /// <summary>
    ///     Actions you can use when state change
    /// </summary>
    public interface IStateAction : IExecute
    {
        /// <summary>
        ///     Use attribute when perform action on state change
        /// </summary>
        /// <param name="name">The name of attribute</param>
        /// <param name="value">The value of attribute</param>
        IStateAction WithAttribute(string name, object value);
    }


    /// <summary>
    ///     When state change
    /// </summary>
    public interface IStateChanged
    {
        /// <summary>
        ///     When state change from or to a state
        /// </summary>
        /// <param name="to">The state change to, or null if any state</param>
        /// <param name="from">The state changed from or null if any state</param>
        /// <param name="allChanges">Get all changed, even only attribute changes</param>
        IState WhenStateChange(object? to = null, object? from = null, bool allChanges = false);

        /// <summary>
        ///     When state change, using a lambda expression
        /// </summary>
        /// <param name="stateFunc">The lambda expression used to track changes</param>
        IState WhenStateChange(Func<EntityState?, EntityState?, bool> stateFunc);
    }

    /// <summary>
    ///     Track states on entities
    /// </summary>
    public interface IStateEntity : ITurnOff<IStateAction>, ITurnOn<IStateAction>, IToggle<IStateAction>,
        ISetState<IStateAction>
    {
    }

    /// <summary>
    ///     Time operations
    /// </summary>
    public interface ITime
    {
        /// <summary>
        ///     Run an action once every period of time
        /// </summary>
        /// <param name="timeSpan">The time between runs</param>
        ITimeItems Every(TimeSpan timeSpan);
    }

    /// <summary>
    ///     Entities used on time actions
    /// </summary>
    public interface ITimeItems
    {
        /// <summary>
        ///     Use entities with lambda expression for further actions
        /// </summary>
        /// <param name="func">Lambda expression to filter out entities</param>
        ITimerEntity Entities(Func<IEntityProperties, bool> func);

        /// <summary>
        ///     Use entities from list
        /// </summary>
        /// <param name="entities">The entities to perform actions on</param>
        ITimerEntity Entities(IEnumerable<string> entities);

        /// <summary>
        ///     Use entity or multiple entities
        /// </summary>
        /// <param name="entityId">Unique id of the entity provided</param>
        ITimerEntity Entity(params string[] entityId);
    }

    /// <summary>
    ///     The actions to perform on time functions
    /// </summary>
    public interface ITimerAction
    {
        /// <summary>
        ///     Excecute commands
        /// </summary>
        void Execute();

        /// <summary>
        ///     Use attribute on entity action in time actions
        /// </summary>
        /// <param name="name">Name of attribute</param>
        /// <param name="value">Value of attribute</param>
        ITimerAction UsingAttribute(string name, object value);
    }

    /// <summary>
    ///     Entity timer actions
    /// </summary>
    public interface ITimerEntity : ITurnOff<ITimerAction>, ITurnOn<ITimerAction>, IToggle<ITimerAction>
    {
    }

    #region Media, Play, Stop, Pause, PlayPause, Speak

    /// <summary>
    ///     Generic interface for pause
    /// </summary>
    /// <typeparam name="T">Return type of pause operation</typeparam>
    public interface IPause<T>
    {
        /// <summary>
        /// Pauses entity
        /// </summary>
        T Pause();
    }

    /// <summary>
    ///     Generic interface for play
    /// </summary>
    /// <typeparam name="T">Return type of play operation</typeparam>
    public interface IPlay<T>
    {
        /// <summary>
        ///     Plays entity
        /// </summary>
        T Play();
    }

    /// <summary>
    ///     Generic interface for playpause
    /// </summary>
    /// <typeparam name="T">Return type of playpause operation</typeparam>
    public interface IPlayPause<T>
    {
        /// <summary>
        ///     Play/Pause entity
        /// </summary>
        T PlayPause();
    }

    /// <summary>
    ///     Generic interface for speak
    /// </summary>
    /// <typeparam name="T">Return type of speak operation</typeparam>
    public interface ISpeak<T>
    {
        /// <summary>
        ///     Speak using entity
        /// </summary>
        T Speak(string message);
    }

    /// <summary>
    ///     Generic interface for stop
    /// </summary>
    /// <typeparam name="T">Return type of stop operation</typeparam>
    public interface IStop<T>
    {
        /// <summary>
        ///     Stops entity
        /// </summary>
        T Stop();
    }
    #endregion


    #region Entities, TurnOn, TurnOff, Toggle

    /// <summary>
    ///     Generic interface for SetState
    /// </summary>
    /// <typeparam name="T">Return type of SetState operation</typeparam>
    public interface ISetState<T>
    {
        /// <summary>
        ///     Set entity state
        /// </summary>
        /// <param name="state">The state to set</param>
        T SetState(dynamic state);
    }

    /// <summary>
    ///     Generic interface for Toggle
    /// </summary>
    /// <typeparam name="T">Return type of Toggle operation</typeparam>
    public interface IToggle<T>
    {
        /// <summary>
        ///     Toggles entity
        /// </summary>
        T Toggle();
    }

    /// <summary>
    ///     Generic interface for TurnOff
    /// </summary>
    /// <typeparam name="T">Return type of TurnOff operation</typeparam>
    public interface ITurnOff<T>
    {
        /// <summary>
        ///     Turn off entity
        /// </summary>
        T TurnOff();
    }

    /// <summary>
    ///     Generic interface for TurnOn
    /// </summary>
    /// <typeparam name="T">Return type of TurnOn operation</typeparam>
    public interface ITurnOn<T>
    {
        /// <summary>
        ///     Turn on entity
        /// </summary>
        T TurnOn();
    }

    #endregion


    /// <summary>
    ///     Implements interface for managing entities in the fluent API
    /// </summary>
    public class EntityManager : EntityState, IEntity, ILight, IAction,
        IStateEntity, IState, IStateAction, IMediaPlayer, IScript, IMediaPlayerExecuteAsync
    {
        private readonly ConcurrentQueue<FluentAction> _actions =
            new ConcurrentQueue<FluentAction>();

        private readonly INetDaemon _daemon;
        private readonly IEnumerable<string> _entityIds;

        private FluentAction? _currentAction;

        private StateChangedInfo _currentState = new StateChangedInfo();


        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="entityIds">The unique ids of the entities managed</param>
        /// <param name="daemon">The Daemon that will handle API calls to Home Assistant</param>
        public EntityManager(IEnumerable<string> entityIds, INetDaemon daemon)
        {
            _entityIds = entityIds;
            _daemon = daemon;
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
        public void Execute()
        {
            foreach (var entityId in _entityIds)
                _daemon.ListenState(entityId, async (entityIdInn, newState, oldState) =>
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
                        if (newState?.State == oldState?.State && !_currentState.AllChanges)
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
                            if (currentState?.LastChanged == newState?.LastChanged)
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
                                    $"State same {newState?.State} but different state changed: {currentState?.LastChanged}, expected {newState?.LastChanged}");
                            }
                        }
                        else
                        {
                            _daemon.Logger.LogDebug(
                                $"State not same, do not execute for statement. {newState?.State} found, expected {currentState?.State}");
                        }
                    }
                    else
                    {
                        _daemon.Logger.LogDebug(
                            $"State {newState?.State} expected from {oldState?.State}, executing action!");

                        if (_currentState.FuncToCall != null)
                            await _currentState.FuncToCall(entityIdInn, newState, oldState);
                        else if (_currentState.ScriptToCall != null)
                            await _daemon.RunScript(_currentState.ScriptToCall).ExecuteAsync();
                        else
                            await entityManager.ExecuteAsync(true);
                    }
                });
            //}
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
        public IMediaPlayerExecuteAsync Speak(string message)
        {
            _currentAction = new FluentAction(FluentActionType.Speak);
            _currentAction.MessageToSpeak = message;
            return this;
        }

        /// <inheritdoc/>
        public IMediaPlayerExecuteAsync Stop()
        {
            _currentAction = new FluentAction(FluentActionType.Stop);
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
            _currentState.Entity = _daemon.Entities(func);
            return this;
        }

        /// <inheritdoc/>
        public IStateEntity UseEntities(IEnumerable<string> entities)
        {
            _currentState.Entity = _daemon.Entities(entities);
            return this;
        }

        /// <inheritdoc/>
        public IStateEntity UseEntity(params string[] entityId)
        {
            _currentState.Entity = _daemon.Entity(entityId);
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
        private static string GetDomainFromEntity(string entity)
        {
            var entityParts = entity.Split('.');
            if (entityParts.Length != 2)
                throw new ApplicationException($"entity_id is mal formatted {entity}");

            return entityParts[0];
        }

        /// <inheritdoc/>
        private async Task CallServiceOnAllEntities(string service)
        {
            var taskList = new List<Task>();
            foreach (var entityId in _entityIds)
            {
                var domain = GetDomainFromEntity(entityId);
                dynamic serviceData = new FluentExpandoObject();
                serviceData.entity_id = entityId;
                var task = _daemon.CallService(domain, service, serviceData);
                taskList.Add(task);
            }

            if (taskList.Count > 0) await Task.WhenAny(Task.WhenAll(taskList.ToArray()), Task.Delay(5000));
        }

        /// <inheritdoc/>
        async Task IMediaPlayerExecuteAsync.ExecuteAsync()
        {
            _ = _currentAction ?? throw new NullReferenceException("Missing fluent action type!");

            var executeTask = _currentAction.ActionType switch
            {
                FluentActionType.Play => CallServiceOnAllEntities("media_play"),
                FluentActionType.Pause => CallServiceOnAllEntities("media_pause"),
                FluentActionType.PlayPause => CallServiceOnAllEntities("media_play_pause"),
                FluentActionType.Stop => CallServiceOnAllEntities("media_stop"),
                FluentActionType.Speak => Speak(),
                _ => throw new NotSupportedException($"Action type not supported {_currentAction.ActionType}")
            };

            await executeTask;

            // Use local function to get the nice switch statement above:)
            async Task Speak()
            {
                foreach (var entityId in _entityIds)
                {
                    var message = _currentAction?.MessageToSpeak ??
                        throw new NullReferenceException("Message to speak is null or empty!");

                    await _daemon.Speak(entityId, message);
                }
            }
        }

        /// <inheritdoc/>
        async Task IExecuteAsync.ExecuteAsync()
        {
            await ExecuteAsync();
        }

        /// <inheritdoc/>
        async Task IScript.ExecuteAsync()
        {
            var taskList = new List<Task>();
            foreach (var scriptName in _entityIds)
            {
                var task = _daemon.CallService("script", scriptName);
                taskList.Add(task);
            }

            // Wait for all tasks to complete or max 5 seconds
            if (taskList.Count > 0) await Task.WhenAny(Task.WhenAll(taskList.ToArray()), Task.Delay(5000));
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

        /// <inheritdoc/>
        public IMediaPlayerExecuteAsync Pause()
        {
            _currentAction = new FluentAction(FluentActionType.Pause);
            return this;
        }

        /// <inheritdoc/>
        public IMediaPlayerExecuteAsync Play()
        {
            _currentAction = new FluentAction(FluentActionType.Play);
            return this;
        }

        /// <inheritdoc/>
        public IMediaPlayerExecuteAsync PlayPause()
        {
            _currentAction = new FluentAction(FluentActionType.PlayPause);
            return this;
        }

        /// <inheritdoc/>
        public IExecute RunScript(params string[] entityIds)
        {
            _currentState.ScriptToCall = entityIds;
            return this;
        }

        /// <inheritdoc/>
        private async Task ExecuteAsync()
        {
            await ExecuteAsync(false);
        }

        /// <inheritdoc/>
        private async Task HandleAction(FluentAction fluentAction)
        {
            var attributes = fluentAction.Attributes.Select(n => (n.Key, n.Value)).ToArray();

            var taskList = new List<Task>();
            foreach (var entityId in _entityIds)
            {
                var task = fluentAction.ActionType switch
                {
                    FluentActionType.TurnOff => _daemon.TurnOffAsync(entityId, attributes),
                    FluentActionType.TurnOn => _daemon.TurnOnAsync(entityId, attributes),
                    FluentActionType.Toggle => _daemon.ToggleAsync(entityId, attributes),
                    FluentActionType.SetState => _daemon.SetState(entityId, fluentAction.State, attributes),
                    _ => throw new NotSupportedException($"Fluent action type not handled! {fluentAction.ActionType}")
                };
                taskList.Add(task);
            }
            // Wait for all tasks to complete or max 5 seconds
            if (taskList.Count > 0) await Task.WhenAny(Task.WhenAll(taskList.ToArray()), Task.Delay(5000));
        }
    }

    /// <summary>
    ///     Handles timer features
    /// </summary>
    public class TimeManager : ITime, ITimeItems, ITimerEntity, ITimerAction
    {
        private readonly INetDaemon _daemon;
        private readonly List<string> _entityIds = new List<string>();
        private TimeSpan _timeSpan;
        private EntityManager? _entityManager;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="daemon">The NetDaemon that handles API calls to Home Assistant</param>
        public TimeManager(INetDaemon daemon)
        {
            _daemon = daemon;
        }

        /// <inheritdoc/>
        public ITimerEntity Entities(Func<IEntityProperties, bool> func)
        {
            var x = _daemon.State.Where(func);
            _entityIds.AddRange(x.Select(n => n.EntityId).ToArray());
            _entityManager = new EntityManager(_entityIds.ToArray(), _daemon);
            return this;
        }

        /// <inheritdoc/>
        public ITimerEntity Entities(IEnumerable<string> entities)
        {
            _entityIds.AddRange(entities);
            _entityManager = new EntityManager(_entityIds, _daemon);
            return this;
        }

        /// <inheritdoc/>
        public ITimerEntity Entity(params string[] entityIds)
        {
            _entityIds.AddRange(entityIds);
            _entityManager = new EntityManager(entityIds, _daemon);
            return this;
        }

        /// <inheritdoc/>
        public ITimeItems Every(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan;
            return this;
        }

        /// <inheritdoc/>
        public void Execute()
        {
            _ = _entityManager ?? throw new NullReferenceException($"{nameof(_entityManager)} can not be null here!");

            _daemon.Scheduler.RunEveryAsync(_timeSpan, async () => { await _entityManager.ExecuteAsync(true); });
        }

        /// <inheritdoc/>
        public ITimerAction Toggle()
        {
            _ = _entityManager ?? throw new NullReferenceException($"{nameof(_entityManager)} can not be null here!");

            _entityManager.Toggle();
            return this;
        }

        /// <inheritdoc/>
        public ITimerAction TurnOff()
        {
            _ = _entityManager ?? throw new NullReferenceException($"{nameof(_entityManager)} can not be null here!");

            _entityManager.TurnOff();
            return this;
        }

        /// <inheritdoc/>
        public ITimerAction TurnOn()
        {
            _ = _entityManager ?? throw new NullReferenceException($"{nameof(_entityManager)} can not be null here!");

            _entityManager.TurnOn();
            return this;
        }

        /// <inheritdoc/>
        public ITimerAction UsingAttribute(string name, object value)
        {
            _ = _entityManager ?? throw new NullReferenceException($"{nameof(_entityManager)} can not be null here!");

            _entityManager.WithAttribute(name, value);
            return this;
        }
    }

    /// <summary>
    ///     Represents data about an action in a fluent API
    /// </summary>
    internal class FluentAction
    {
        // Todo: refactor the action class to manage only data that is relevant

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="type">The type of action that is being managed</param>
        public FluentAction(FluentActionType type)
        {
            ActionType = type;
            Attributes = new Dictionary<string, object>();
        }

        /// <summary>
        ///     Type of action
        /// </summary>
        public FluentActionType ActionType { get; }

        /// <summary>
        ///     Attributes used in action if specified
        /// </summary>
        public Dictionary<string, object> Attributes { get; }

        /// <summary>
        ///     Message to speak if it is a speak action
        /// </summary>
        public string? MessageToSpeak { get; internal set; }

        /// <summary>
        ///     The state to manage for state actions
        /// </summary>
        public dynamic? State { get; set; }
    }

    /// <summary>
    ///     Information about state changed actions
    /// </summary>
    internal class StateChangedInfo
    {
        /// <summary>
        ///     All changes tracked, even if only attributes
        /// </summary>
        public bool AllChanges { get; set; }

        /// <summary>
        ///     Entity changed
        /// </summary>
        public IEntity? Entity { get; set; }

        /// <summary>
        ///     Timespan it have kept same state
        /// </summary>
        public TimeSpan ForTimeSpan { get; set; }

        /// <summary>
        ///     From state
        /// </summary>
        public dynamic? From { get; set; }

        /// <summary>
        ///     To state
        /// </summary>
        public dynamic? To { get; set; }

        /// <summary>
        ///     Function to call when state changes
        /// </summary>
        public Func<string, EntityState?, EntityState?, Task>? FuncToCall { get; set; }

        /// <summary>
        ///     Filter state changes with lamda
        /// </summary>
        public Func<EntityState?, EntityState?, bool>? Lambda { get; set; }

        /// <summary>
        ///     Script to call when state changes
        /// </summary>
        public string[]? ScriptToCall { get; internal set; }
    }
}