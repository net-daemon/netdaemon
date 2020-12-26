using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetDaemon.Common.Exceptions;

namespace NetDaemon.Common.Fluent
{
    /// <summary>
    ///     Base class for entity fluent types
    /// </summary>
    public class EntityBase //: EntityState
    {
        internal readonly ConcurrentQueue<FluentAction> _actions =
            new();

        internal FluentAction? _currentAction;

        internal StateChangedInfo _currentState = new();

        /// <summary>
        ///     The daemon used in the API
        /// </summary>
        protected INetDaemonApp App { get; }

        /// <summary>
        ///     The daemon used in the API
        /// </summary>
        protected INetDaemon Daemon { get; }

        /// <summary>
        ///     The EntityIds used
        /// </summary>
        protected IEnumerable<string> EntityIds { get; }

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
            if (string.IsNullOrEmpty(entity))
                throw new NetDaemonNullReferenceException(nameof(entity));

            var entityParts = entity.Split('.');
            if (entityParts.Length != 2)
                throw new NetDaemonException($"entity_id is mal formatted {entity}");

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
                var task = Daemon.CallServiceAsync(domain, service, serviceData);
                taskList.Add(task);
            }

            if (taskList.Count > 0) await Task.WhenAny(Task.WhenAll(taskList.ToArray()), Task.Delay(5000)).ConfigureAwait(false);
        }
    }
}