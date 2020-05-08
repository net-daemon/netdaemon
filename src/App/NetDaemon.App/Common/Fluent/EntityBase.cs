using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    /// <summary>
    ///     Base class for entity fluent types
    /// </summary>
    public class EntityBase : EntityState
    {
        internal readonly ConcurrentQueue<FluentAction> _actions =
            new ConcurrentQueue<FluentAction>();

        internal FluentAction? _currentAction;

        internal StateChangedInfo _currentState = new StateChangedInfo();

        /// <summary>
        ///     The daemon used in the API
        /// </summary>
        protected readonly INetDaemonApp App;

        /// <summary>
        ///     The daemon used in the API
        /// </summary>
        protected readonly INetDaemon Daemon;

        /// <summary>
        ///     The EntityIds used
        /// </summary>
        protected readonly IEnumerable<string> EntityIds;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="entityIds">The unique ids of the entities managed</param>
        /// <param name="daemon">The Daemon that will handle API calls to Home Assistant</param>
        /// <param name="app">The Daemon App calling fluent API</param>
        public EntityBase(IEnumerable<string> entityIds, INetDaemon daemon, INetDaemonApp app)
        {
            EntityIds = entityIds;
            Daemon = daemon;
            App = app;
        }

        /// <inheritdoc/>
        protected static string GetDomainFromEntity(string entity)
        {
            var entityParts = entity.Split('.');
            if (entityParts.Length != 2)
                throw new ApplicationException($"entity_id is mal formatted {entity}");

            return entityParts[0];
        }

        /// <inheritdoc/>
        protected async Task CallServiceOnAllEntities(string service, dynamic? serviceData = null)
        {
            var taskList = new List<Task>();
            foreach (var entityId in EntityIds)
            {
                var domain = GetDomainFromEntity(entityId);
                serviceData ??= new FluentExpandoObject();
                serviceData.entity_id = entityId;
                var task = Daemon.CallService(domain, service, serviceData);
                taskList.Add(task);
            }

            if (taskList.Count > 0) await Task.WhenAny(Task.WhenAll(taskList.ToArray()), Task.Delay(5000)).ConfigureAwait(false);
        }
    }
}