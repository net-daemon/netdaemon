using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using NetDaemon.Common.Fluent;
using NetDaemon.Common.Reactive;

namespace NetDaemon.Common.Services
{
    public partial class PersonEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public EntityState? EntityState => DaemonRxApp?.State(EntityId);
        public string? Area => DaemonRxApp?.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp?.State(EntityId)?.Attribute;
        public DateTime? LastChanged => DaemonRxApp?.State(EntityId)?.LastChanged;
        public DateTime? LastUpdated => DaemonRxApp?.State(EntityId)?.LastUpdated;
        public dynamic? State => DaemonRxApp?.State(EntityId)?.State;
        public PersonEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void Reload(dynamic? data = null)
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

            DaemonRxApp.CallService("person", "reload", serviceData);
        }
    }
}