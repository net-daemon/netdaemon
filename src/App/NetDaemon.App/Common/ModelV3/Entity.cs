using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using NetDaemon.Common.Reactive;

namespace NetDaemon.Common.Model
{

    public abstract class Entity<TEntityState> : Entity where TEntityState : IEntityProperties
    {
        protected Entity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        { }

        public new IObservable<(TEntityState Old, TEntityState New)> StateChanges
            => base.StateChanges.Select(e => (MapEntityState(e.Old), MapEntityState(e.New)));

        public new IObservable<(TEntityState Old, TEntityState New)> StateAllChanges
            => base.StateAllChanges.Select(e => (MapEntityState(e.Old), MapEntityState(e.New)));

        public TEntityState? EntityState => MapEntityState(HaContext.GetState<TEntityState>(EntityIds.FirstOrDefault()));

        protected abstract TEntityState MapEntityState(IEntityProperties state);
    }

    public class Entity : IEntity
    {
        /// <summary>
        ///     The IHAContext
        /// </summary>
        protected IHaContext HaContext { get; }

        /// <summary>
        ///     Entity ids being handled by the RxEntity
        /// </summary>
        public IEnumerable<string> EntityIds { get; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="daemon">The NetDaemon host object</param>
        /// <param name="entityIds">Unique entity id:s</param>
        public Entity(IHaContext daemon, IEnumerable<string> entityIds)
        {
            HaContext = daemon;
            EntityIds = entityIds;
        }

        public Entity(IHaContext daemon, string entityId)
        {
            HaContext = daemon;
            EntityIds = new [] { entityId };
        }

        public void SetState(dynamic state, dynamic? attributes = null, bool waitForResponse = false)
        {
            foreach (var entityId in EntityIds)
            {
                HaContext.SetState(entityId, state, attributes, waitForResponse);
            }
        }

        public IObservable<(IEntityProperties Old, IEntityProperties New)> StateAllChanges =>
            HaContext.StateAllChanges.Where(e => EntityIds.Contains(e.New.EntityId)).Select(e => (e.Old as IEntityProperties, e.New as IEntityProperties));

        public IObservable<(IEntityProperties Old, IEntityProperties New)> StateChanges =>
            HaContext.StateChanges.Where(e => EntityIds.Contains(e.New.EntityId)).Select(e => (e.Old as IEntityProperties, e.New as IEntityProperties));

        public void CallService(string service, object? data = null, bool waitForResponse = false)
        {
            foreach (var entityId in EntityIds!)
            {
                var serviceData = new FluentExpandoObject();

                if (data is ExpandoObject)
                {
                    // Make sure we make a copy since we reuse all info but entity id
                    serviceData.CopyFrom((dynamic)data);
                }
                else
                {
                    // It is initialized with anonymous type new {transition=10} for example
                    var expObject = data?.ToExpandoObject();
                    if (expObject != null)
                    {
                        serviceData.CopyFrom(expObject);
                    }
                }

                var domain = entityId.SplitEntityId().Domain;

                serviceData["entity_id"] = entityId;

                HaContext.CallService(domain, service, serviceData, waitForResponse);
            }
        }
    }
}
