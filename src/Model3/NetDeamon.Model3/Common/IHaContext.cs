using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using NetDaemon.Model3.Entities;

namespace NetDaemon.Model3.Common
{
    /// <summary>
    /// Represents a context for interacting with Home Assistant
    /// </summary>
    public interface IHaContext
    {
        /// <summary>
        ///     The observable statestream, all changes including attributes
        /// </summary>
        IObservable<StateChange> StateAllChanges { get; }

        /// <summary>
        ///     The observable statestream state change
        /// </summary>
        /// <remarks>
        ///     Old state != New state
        /// </remarks>
        IObservable<StateChange> StateChanges => StateAllChanges.Where(e => e.New?.State != e.Old?.State);

        /// <summary>
        ///     Get state for a single entity
        /// </summary>
        /// <param name="entityId"></param>
        EntityState? GetState(string entityId);

        /// <summary>
        /// Gets all the entities in HomeAssistant
        /// </summary>
        IReadOnlyList<Entity> GetAllEntities();

        /// <summary>
        ///     Calls service in Home Assistant
        /// </summary>
        /// <param name="domain">Domain of service</param>
        /// <param name="service">Service name</param>
        /// <param name="target">The target that is targeted by this service call</param>
        /// <param name="data">Data provided to service. Use anonyomous type</param>
        void CallService(string domain, string service, ServiceTarget? target = null, object? data = null);
    }
}
