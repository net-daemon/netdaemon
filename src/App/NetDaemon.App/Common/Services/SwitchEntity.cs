using System;
using System.Collections.Generic;
using System.Linq;
using NetDaemon.Common.Reactive;

namespace NetDaemon.Common.Services
{
    public partial class SwitchEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public EntityState? EntityState => DaemonRxApp?.State(EntityId);
        public string? Area => DaemonRxApp?.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp?.State(EntityId)?.Attribute;
        public DateTime? LastChanged => DaemonRxApp?.State(EntityId)?.LastChanged;
        public DateTime? LastUpdated => DaemonRxApp?.State(EntityId)?.LastUpdated;
        public dynamic? State => DaemonRxApp?.State(EntityId)?.State;
        public SwitchEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }
}