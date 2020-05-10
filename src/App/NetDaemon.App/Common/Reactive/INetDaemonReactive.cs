using System;
using System.Collections.Generic;

namespace JoySoftware.HomeAssistant.NetDaemon.Common.Reactive
{
    public interface ICallService
    {
        void CallService(string domain, string service, dynamic? data);
    }

    /// <summary>
    ///     Implements the System.Reactive pattern for NetDaemon Apps
    /// </summary>
    public interface INetDaemonReactive : INetDaemonAppBase, ICallService, IRxEntity
    {
        /// <summary>
        ///     The observable statestream
        /// </summary>
        public IRxStateChange StateChanges { get; }

        /// <summary>
        ///     Enuberable of current states
        /// </summary>
        IEnumerable<EntityState> States { get; }

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

    public interface IRxSchedule
    {
        /// <summary>
        ///     Shedules an action every (timespan) 
        /// </summary>
        /// <param name="timespan">The timespan to schedule</param>
        IObservable<long> RunEvery(TimeSpan timespan);

        /// <summary>
        ///     Run daily at a specific time
        /// </summary>
        /// <param name="time">The time in "hh:mm:ss" format</param>
        IObservable<long> RunDaily(string time);

        /// <summary>
        ///     Delays excecution of an action (timespan) time
        /// </summary>
        /// <param name="timespan">Timespan to delay</param>
        IObservable<long> RunIn(TimeSpan timespan);
    }

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

    public interface IRxStateChange : IObservable<(EntityState Old, EntityState New)>
    {
    }
}