using System;
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
        IObservable<StateChange> StateChanges { get; }

        /// <summary>
        ///     Get state for a single entity
        /// </summary>
        /// <param name="entityId"></param>
        EntityState? GetState(string entityId);

        /// <summary>
        ///     Calls service in Home Assistant
        /// </summary>
        /// <param name="domain">Domain of service</param>
        /// <param name="service">Service name</param>
        /// <param name="data">Data provided to service. Should deserialize into correct json using System.Text.Json</param>
        /// <param name="entity">The entity that is targeted by this service call</param>
        void CallService(string domain, string service, object? data, Entity? entity);
    }
}