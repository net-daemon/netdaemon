using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace JoySoftware.HomeAssistant.NetDaemon.Common.Reactive
{
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
    ///     Implements the entity of Rx API
    /// </summary>
    public class RxEntity : ICanTurnOnAndOff, ISetState, IObserve
    {
        private readonly INetDaemon _daemon;
        private readonly IEnumerable<string> _entityIds;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="daemon">The NetDaemon host object</param>
        /// <param name="entityIds">Unique entity id:s</param>
        public RxEntity(INetDaemon daemon, IEnumerable<string> entityIds)
        {
            _daemon = daemon;
            _entityIds = entityIds;
        }

        /// <inheritdoc/>
        public IObservable<(EntityState Old, EntityState New)> StateAllChanges
        {
            get
            {
                return _daemon.StateChanges.Where(f => _entityIds.Contains(f.New.EntityId));
            }
        }

        /// <inheritdoc/>
        public IObservable<(EntityState Old, EntityState New)> StateChanges
        {
            get
            {
                return _daemon.StateChanges.Where(f => _entityIds.Contains(f.New.EntityId) && f.New.State != f.Old.State);
            }
        }

        /// <inheritdoc/>
        public void SetState(dynamic state, dynamic? attributes = null)
        {
            foreach (var entityId in _entityIds)
            {
                var domain = GetDomainFromEntity(entityId);
                _daemon.SetState(entityId, state, attributes);
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

        private void CallServiceOnEntity(string service, dynamic? attributes = null)
        {
            if (_entityIds is null || _entityIds is object && _entityIds.Count() == 0)
                return;

            dynamic? data = null;

            if (attributes is object)
            {
                if (attributes is IDictionary<string, object?> == false)
                    data = ((object)attributes).ToExpandoObject();
                else
                    data = attributes;
            }

            foreach (var entityId in _entityIds!)
            {
                var serviceData = new FluentExpandoObject();

                if (data is object)
                {
                    // Maske sure we make a copy since we reuse all info but entity id
                    serviceData.CopyFrom(data);
                }

                var domain = GetDomainFromEntity(entityId);

                serviceData["entity_id"] = entityId;

                _daemon.CallService(domain, service, serviceData);
            }
        }
    }
}