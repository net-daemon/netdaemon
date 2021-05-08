using System;
using System.Collections.Generic;
using System.Threading;
using NetDaemon.Common.Reactive;

namespace NetDaemon.Common.Model
{
    public interface IHaContext
    {
        /// <summary>
        ///     The observable events
        /// </summary>
        IRxEvent EventChanges { get; }

        /// <summary>
        ///     The observable statestream, all changes inkluding attributes
        /// </summary>
        IObservable<(EntityState Old, EntityState New)> StateAllChanges { get; }

        /// <summary>
        ///     The observable statestream state change
        /// </summary>
        /// <remarks>
        ///     Old state != New state
        /// </remarks>
        IObservable<(EntityState Old, EntityState New)> StateChanges { get; }

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
        /// <param name="waitForResponse">If true it waits for response and returns new state</param>
        EntityState? SetState(string entityId, object state, object? attributes, bool waitForResponse = false);

        /// <summary>
        ///     Get state for a single entity
        /// </summary>
        /// <param name="entityId"></param>
        TState? GetState<TState>(string entityId);

        /// <summary>
        ///     Calls service in Home Assistant
        /// </summary>
        /// <param name="domain">Domain of sevice</param>
        /// <param name="service">Service name</param>
        /// <param name="data">Data provided to service. Use anonomous type</param>
        /// <param name="waitForResponse">Waits for Home Assistant to return result before returning</param>
        void CallService(string domain, string service, object? data, bool waitForResponse = false);
    }
}