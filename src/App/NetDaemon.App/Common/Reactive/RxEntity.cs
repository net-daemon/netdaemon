using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reactive.Linq;
using NetDaemon.Common.Fluent;

namespace NetDaemon.Common.Reactive
{
    /// <summary>
    ///     Interface for objects implements Toggle
    /// </summary>
    public interface ICanTurnOnAndOff
    {
        /// <summary>
        ///     Toggles state on/off on entity
        /// </summary>
        /// <param name="attributes">The attributes to set. Use anonomous type</param>
        void Toggle(dynamic? attributes = null);

        /// <summary>
        ///     Turn off entity
        /// </summary>
        /// <param name="attributes">The attributes to set. Use anonomous type.</param>
        void TurnOff(dynamic? attributes = null);

        /// <summary>
        ///     Turn on entity
        /// </summary>
        /// <param name="attributes">The attributes to set. Use anonomous type.</param>
        void TurnOn(dynamic? attributes = null);
    }

    /// <summary>
    ///     Interface for objects exposing observable state changes
    /// </summary>
    public interface IObserve
    {
        /// <summary>
        ///     Observable, All state changes inkluding attributes
        /// </summary>
        IObservable<(EntityState Old, EntityState New)> StateAllChanges { get; }

        /// <summary>
        ///     Observable, All state changes. New.State!=Old.State
        /// </summary>
        IObservable<(EntityState Old, EntityState New)> StateChanges { get; }
    }

    /// <summary>
    ///     Interface for objects implements SetState
    /// </summary>
    public interface ISetState
    {
        /// <summary>
        ///     Set entity state
        /// </summary>
        /// <param name="state">The state to set, primitives only</param>
        /// <param name="attributes">The attributes to set. Use anonomous type</param>
        void SetState(dynamic state, dynamic? attributes = null);
    }

    /// <summary>
    ///     Implements the entity of Rx API
    /// </summary>
    public class RxEntity : ICanTurnOnAndOff, ISetState, IObserve
    {
        /// <summary>
        ///     The protected daemon app instance
        /// </summary>
        protected readonly INetDaemonReactive DaemonRxApp;
        /// <summary>
        ///     Entity ids being handled by the RxEntity
        /// </summary>
        protected readonly IEnumerable<string> EntityIds;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="daemon">The NetDaemon host object</param>
        /// <param name="entityIds">Unique entity id:s</param>
        public RxEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds)
        {
            DaemonRxApp = daemon;
            EntityIds = entityIds;
        }

        /// <inheritdoc/>
        public IObservable<(EntityState Old, EntityState New)> StateAllChanges
        {
            get
            {
                return DaemonRxApp.StateAllChanges.Where(f => EntityIds.Contains(f.New.EntityId));
            }
        }

        /// <inheritdoc/>
        public IObservable<(EntityState Old, EntityState New)> StateChanges
        {
            get
            {
                return DaemonRxApp.StateChanges.Where(f => EntityIds.Contains(f.New.EntityId) && f.New?.State != f.Old?.State);
            }
        }

        /// <inheritdoc/>
        public void SetState(dynamic state, dynamic? attributes = null)
        {
            foreach (var entityId in EntityIds)
            {
                var domain = GetDomainFromEntity(entityId);
                DaemonRxApp.SetState(entityId, state, attributes);
            }
        }

        /// <inheritdoc/>
        public void Toggle(dynamic? attributes = null) => CallServiceOnEntity("toggle", attributes);

        /// <inheritdoc/>
        public void TurnOff(dynamic? attributes = null) => CallServiceOnEntity("turn_off", attributes);

        /// <inheritdoc/>
        public void TurnOn(dynamic? attributes = null) => CallServiceOnEntity("turn_on", attributes);

        internal static string GetDomainFromEntity(string entity)
        {
            var entityParts = entity.Split('.');
            if (entityParts.Length != 2)
                throw new ApplicationException($"entity_id is mal formatted {entity}");

            return entityParts[0];
        }

        /// <summary>
        ///     Calls a service using current entity id/s and the entity domain
        /// </summary>
        /// <param name="service">Name of the service to call</param>
        /// <param name="data">Data to provide</param>
        public void CallService(string service, dynamic? data = null)
        {
            if (EntityIds is null || EntityIds is not null && EntityIds.Count() == 0)
                return;

            foreach (var entityId in EntityIds!)
            {
                var serviceData = new FluentExpandoObject();

                if (data is ExpandoObject)
                {
                    // Maske sure we make a copy since we reuse all info but entity id
                    serviceData.CopyFrom(data);
                }
                else if (data is not null)
                {
                    // It is initialized with anonmous type new {transition=10} for example
                    var expObject = ((object)data).ToExpandoObject();
                    if (expObject is not null)
                    {
                        serviceData.CopyFrom(expObject);
                    }
                }

                var domain = GetDomainFromEntity(entityId);

                serviceData["entity_id"] = entityId;

                DaemonRxApp.CallService(domain, service, serviceData);
            }
        }

        private void CallServiceOnEntity(string service, dynamic? attributes = null)
        {
            if (EntityIds is null || EntityIds is not null && EntityIds.Count() == 0)
                return;

            dynamic? data = null;

            if (attributes is not null)
            {
                if (attributes is IDictionary<string, object?> == false)
                    data = ((object)attributes).ToExpandoObject();
                else
                    data = attributes;
            }

            foreach (var entityId in EntityIds!)
            {
                var serviceData = new FluentExpandoObject();

                if (data is not null)
                {
                    // Maske sure we make a copy since we reuse all info but entity id
                    serviceData.CopyFrom(data);
                }

                var domain = GetDomainFromEntity(entityId);

                serviceData["entity_id"] = entityId;

                DaemonRxApp.CallService(domain, service, serviceData);
            }
        }
    }
}