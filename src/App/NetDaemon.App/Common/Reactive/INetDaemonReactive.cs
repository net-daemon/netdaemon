using System;
using System.Collections.Generic;
using System.Threading;

namespace NetDaemon.Common.Reactive
{
    /// <summary>
    ///     Implements the System.Reactive pattern for NetDaemon Apps
    /// </summary>
    public interface INetDaemonRxApp : INetDaemonAppBase, IRxSchedule, IRxEntity
    {
        /// <summary>
        ///     The observable events
        /// </summary>
        public IRxEvent EventChanges { get; }

        /// <summary>
        ///     The observable statestream, all changes including attributes
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
        /// <param name="waitForResponse">If true it waits for response and returns new state</param>
        EntityState? SetState(string entityId, dynamic state, dynamic? attributes, bool waitForResponse = false);

        /// <summary>
        ///     Get state for a single entity
        /// </summary>
        /// <param name="entityId"></param>
        EntityState? State(string entityId);

        /// <summary>
        ///     Calls service in Home Assistant
        /// </summary>
        /// <param name="domain">Domain of sevice</param>
        /// <param name="service">Service name</param>
        /// <param name="data">Data provided to service. Use anonomous type</param>
        /// <param name="waitForResponse">Waits for Home Assistant to return result before returning</param>
        void CallService(string domain, string service, dynamic? data, bool waitForResponse = false);

        /// <summary>
        ///     Calls service in Home Assistant
        /// </summary>
        /// <param name="script">Script to call</param>
        void RunScript(params string[] script);

        /// <summary>
        ///     Delays timeout time
        /// </summary>
        /// <remarks>
        ///     When the app stops it will cancel wait with OperationanceledException
        /// </remarks>
        /// <param name="timeout">Time to delay execution</param>
        void Delay(TimeSpan timeout);

        /// <summary>
        ///     Delays timeout time
        /// </summary>
        /// <remarks>
        ///     When the app stops it will cancel wait with OperationanceledException
        /// </remarks>
        /// <param name="timeout">Time to delay execution</param>
        /// <param name="token">Token to cancel any delays</param>
        void Delay(TimeSpan timeout, CancellationToken token);
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
        IRxEntityBase Entities(Func<IEntityProperties, bool> func);

        /// <summary>
        ///     Entities to perform actions on
        /// </summary>
        /// <param name="entityIds">List of entities</param>
        IRxEntityBase Entities(IEnumerable<string> entityIds);

        /// <summary>
        ///     Entities to perform actions on
        /// </summary>
        /// <param name="entityIds">List of entities</param>
        IRxEntityBase Entities(params string[] entityIds);

        /// <summary>
        ///     Entity to perform actions on
        /// </summary>
        /// <param name="entityId">EntityId</param>
        IRxEntityBase Entity(string entityId);
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
        /// <param name="action">Action to run</param>
        IDisposable RunDaily(string time, Action action);

        /// <summary>
        ///     Shedules an action every (timespan)
        /// </summary>
        /// <param name="timespan">The timespan to schedule</param>
        /// <param name="action">Action to run</param>
        IDisposable RunEvery(TimeSpan timespan, Action action);

        /// <summary>
        ///     Shedules an action every (timespan)
        /// </summary>
        /// <param name="time">The time in "mm:ss" format</param>
        /// <param name="action">Action to run</param>
        IDisposable RunEveryHour(string time, Action action);

        /// <summary>
        ///     Shedules an action every (timespan)
        /// </summary>
        /// <param name="second">The timespan to schedule</param>
        /// <param name="action">Action to run</param>
        IDisposable RunEveryMinute(short second, Action action);

        /// <summary>
        ///     Delays excecution of an action (timespan) time
        /// </summary>
        /// <param name="timespan">Timespan to delay</param>
        /// <param name="action">Action to run</param>
        IDisposable RunIn(TimeSpan timespan, Action action);
    }
}