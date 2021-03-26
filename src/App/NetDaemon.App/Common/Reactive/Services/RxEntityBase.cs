using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace NetDaemon.Common.Reactive.Services
{
    public abstract class RxEntityBase : RxEntity
    {
        /// <summary>
        /// Gets the id of the entity
        /// </summary>
        public string EntityId => EntityIds.First();

        /// <summary>
        /// Gets the entity state
        /// </summary>
        public virtual EntityState? EntityState => DaemonRxApp?.State(EntityId);

        /// <summary>
        /// Gets the Area to which an entity is assigned
        /// </summary>
        public string? Area => DaemonRxApp?.State(EntityId)?.Area;

        /// <summary>
        /// Gets the entity attribute
        /// </summary>
        public dynamic? Attribute => DaemonRxApp?.State(EntityId)?.Attribute;

        /// <summary>
        /// Gets a <see cref="DateTime"/> that indicates the last time the entity's state changed
        /// </summary>
        public DateTime? LastChanged => DaemonRxApp?.State(EntityId)?.LastChanged;

        /// <summary>
        ///  Gets a <see cref="DateTime"/> that indicates the last time the entity's state updated
        /// </summary>
        public DateTime? LastUpdated => DaemonRxApp?.State(EntityId)?.LastUpdated;

        /// <summary>
        /// Gets the entity's state
        /// </summary>
        public dynamic? State => DaemonRxApp?.State(EntityId)?.State;

        /// <summary>
        /// Representing an AlarmControlPanel entity.
        /// </summary>
        /// <param name="daemon">An instance of <see cref="INetDaemonRxApp"/>.</param>
        /// <param name="entityIds">A list of entity id's that represent this entity</param>
        protected RxEntityBase(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        /// Performs a specified service call to a specified domain with specified data
        /// </summary>
        /// <param name="domain">The domain to which the service call belongs</param>
        /// <param name="service">The service in the domain to call</param>
        /// <param name="data">Additional data to send to the service</param>
        /// <param name="sendEntityId">If true it will include the entity_id attribute with this entity's EntityId with the service call</param>
        protected void CallService(string domain, string service, dynamic? data = null, bool sendEntityId = false)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            if (sendEntityId)
                serviceData["entity_id"] = EntityId;

            DaemonRxApp.CallService(domain, service, serviceData);
        }
    }
}