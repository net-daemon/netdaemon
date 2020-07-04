using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        Speak,
        DelayUntilStateChange
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
    ///     When state change
    /// </summary>
    public interface IDelayStateChange
    {
        /// <summary>
        ///     When state change from or to a state
        /// </summary>
        /// <param name="to">The state change to, or null if any state</param>
        /// <param name="from">The state changed from or null if any state</param>
        /// <param name="allChanges">Get all changed, even only attribute changes</param>
        IDelayResult DelayUntilStateChange(object? to = null, object? from = null, bool allChanges = false);

        /// <summary>
        ///     When state change, using a lambda expression
        /// </summary>
        /// <param name="stateFunc">The lambda expression used to track changes</param>
        IDelayResult DelayUntilStateChange(Func<EntityState?, EntityState?, bool> stateFunc);
    }

    /// <summary>
    ///     Represents an entity
    /// </summary>
    public interface IEntity :
        ITurnOff<IAction>, ITurnOn<IAction>, IToggle<IAction>,
        IStateChanged, ISetState<IAction>, IDelayStateChange
    { }

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
    ///     Expressions that ends with async execution
    /// </summary>
    public interface IExecuteAsync
    {
        /// <summary>
        ///     Execute action async
        /// </summary>
        Task ExecuteAsync();
    }

    /// <summary>
    ///     Represents a script entity
    /// </summary>
    public interface IScript
    {
        /// <summary>
        ///     Executes scripts async
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

    #endregion Media, Play, Stop, Pause, PlayPause, Speak

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

    #endregion Entities, TurnOn, TurnOff, Toggle
}