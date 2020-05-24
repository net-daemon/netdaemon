using System;
using System.Collections.Generic;

namespace JoySoftware.HomeAssistant.NetDaemon.Common.Reactive
{
    /// <summary>
    ///     Interface for objects that implements CallService
    /// </summary>
    public interface ICallService
    {
        /// <summary>
        ///     Calls service in Home Assistant
        /// </summary>
        /// <param name="domain">Domain of sevice</param>
        /// <param name="service">Service name</param>
        /// <param name="data">Data provided to service. Use anonomous type</param>
        void CallService(string domain, string service, dynamic? data);
    }

    /// <summary>
    ///     Implements the System.Reactive pattern for NetDaemon Apps
    /// </summary>
    public interface INetDaemonReactive : INetDaemonAppBase, ICallService, IRxEntity, IRunScript
    {
        /// <summary>
        ///     The observable events
        /// </summary>
        public IRxEvent EventChanges { get; }

        /// <summary>
        ///     The observable statestream, all changes inkluding attributes
        /// </summary>
        public IObservable<(EntityState Old, EntityState New)> StateAllChanges { get; }

        /// <summary>
        ///     The observable statestream state change
        /// </summary>
        /// <remarks>
        ///     Old state != New state
        /// </remarks>
        public IObservable<(EntityState Old, EntityState New)> StateChanges { get; }

        /// <summary>
        ///     Enuberable of current states
        /// </summary>
        IEnumerable<EntityState> States { get; }

        /// <summary>
        ///     Loads persistent data from unique id
        /// </summary>
        /// <param name="id">Unique Id of the data</param>
        /// <returns>The data persistent or null if not exists</returns>
        T? GetData<T>(string id) where T : class;

        /// <summary>
        ///     Saves any data with unique id, data have to be json serializable
        /// </summary>
        /// <param name="id">Unique id for all apps</param>
        /// <param name="data">Dynamic data being saved</param>
        void SaveData<T>(string id, T data);

        /// <summary>
        ///     Sets a state for entity
        /// </summary>
        /// <param name="entityId">EntityId</param>
        /// <param name="state">The state to set</param>
        /// <param name="attributes">The attributes, use anonomous type like new {attr="someattr", attr2=25}</param>
        void SetState(string entityId, dynamic state, dynamic? attributes);

        /// <summary>
        ///     Get state for a single entity
        /// </summary>
        /// <param name="entityId"></param>
        EntityState? State(string entityId);
    }

    /// <summary>
    ///     Interface for objects that implements CallService
    /// </summary>
    public interface IRunScript
    {
        /// <summary>
        ///     Calls service in Home Assistant
        /// </summary>
        /// <param name="script">Script to call</param>
        void RunScript(params string[] script);
    }

    /// <summary>
    ///     Interface for entities in Rx API
    /// </summary>
    public interface IRxEntity
    {
        /// <summary>
        ///     Select entities to perform actions on
        /// </summary>
        /// <param name="func">Lambda expression</param>
        RxEntity Entities(Func<IEntityProperties, bool> func);

        /// <summary>
        ///     Entities to perform actions on
        /// </summary>
        /// <param name="entityIds">List of entities</param>
        RxEntity Entities(IEnumerable<string> entityIds);

        /// <summary>
        ///     Entities to perform actions on
        /// </summary>
        /// <param name="entityIds">List of entities</param>
        RxEntity Entities(params string[] entityIds);

        /// <summary>
        ///     Entity to perform actions on
        /// </summary>
        /// <param name="entityId">EntityId</param>
        RxEntity Entity(string entityId);
    }

    /// <summary>
    ///     Interface for Observable Events
    /// </summary>
    public interface IRxEvent : IObservable<RxEvent>
    {
    }

    /// <summary>
    ///     Interface for scheduling
    /// </summary>
    public interface IRxSchedule
    {
        /// <summary>
        ///     Run daily at a specific time
        /// </summary>
        /// <param name="time">The time in "hh:mm:ss" format</param>
        IObservable<long> RunDaily(string time);

        /// <summary>
        ///     Shedules an action every (timespan)
        /// </summary>
        /// <param name="timespan">The timespan to schedule</param>
        IObservable<long> RunEvery(TimeSpan timespan);

        /// <summary>
        ///     Shedules an action every (timespan)
        /// </summary>
        /// <param name="time">The time in "mm:ss" format</param>
        IObservable<long> RunEveryHour(string time);

        /// <summary>
        ///     Shedules an action every (timespan)
        /// </summary>
        /// <param name="second">The timespan to schedule</param>
        IObservable<long> RunEveryMinute(short second);

        /// <summary>
        ///     Delays excecution of an action (timespan) time
        /// </summary>
        /// <param name="timespan">Timespan to delay</param>
        IObservable<long> RunIn(TimeSpan timespan);
    }
}