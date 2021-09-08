using System;
using JoySoftware.HomeAssistant.Model;
using NetDaemon.Model3.Entities;

namespace NetDaemon.Model3.Common
{
    public interface IHaContext
    {
        /// <summary>
        ///     The observable events
        /// </summary>
//        IRxEvent EventChanges { get; }

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
        /// <param name="domain">Domain of sevice</param>
        /// <param name="service">Service name</param>
        /// <param name="target">The target that is targeted by this service call</param>
        /// <param name="data">Data provided to service. Use anonomous type</param>
        void CallService(string domain, string service, HassTarget? target = null, object? data = null);
    }
}