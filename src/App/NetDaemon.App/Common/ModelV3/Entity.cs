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

    public abstract class Entity<TEntityState> : IEntity<TEntityState> where TEntityState : IEntityProperties
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

        public TEntityState? EntityState => HaContext.GetState<TEntityState>(EntityIds.FirstOrDefault());


        public void SetState(dynamic state, dynamic? attributes = null, bool waitForResponse = false)
        {
            foreach (var entityId in EntityIds)
            {
                HaContext.SetState(entityId, state, attributes, waitForResponse);
            }
        }

        protected abstract TEntityState MapState(IEntityProperties state);

        public IObservable<(TEntityState Old, TEntityState New)> StateAllChanges =>
            HaContext.StateAllChanges.Where(e => EntityIds.Contains(e.New.EntityId))
                .Select(e => (MapState(e.Old), MapState(e.New)));

        public IObservable<(TEntityState Old, TEntityState New)> StateChanges =>
            HaContext.StateChanges.Where(e => EntityIds.Contains(e.New.EntityId))
                .Select(e => (MapState(e.Old), MapState(e.New)));


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

    public class Entity : Entity<IEntityProperties>
    {
        public Entity(IHaContext daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        public Entity(IHaContext daemon, string entityId) : base(daemon, entityId)
        {
        }

        protected override IEntityProperties MapState(IEntityProperties state) => state;
    }

}
