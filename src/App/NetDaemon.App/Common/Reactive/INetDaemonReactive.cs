using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

namespace JoySoftware.HomeAssistant.NetDaemon.Common.Reactive
{
    public interface ICallService
    {
        void CallService(string domain, string service, dynamic? data);
    }

    

    /// <summary>
    ///     Implements the System.Reactive pattern for NetDaemon Apps
    /// </summary>
    public interface INetDaemonReactive : INetDaemonAppBase, ICallService, IRxEntity
    //IObservable<EntityState>

    {
        public IRxStateChange StateChanges { get; }

        IEnumerable<EntityState> States { get; }

        RxEntity Entity(string entityId);
        EntityState? State(string entityId);
        void SetState(string entityId, dynamic state, dynamic? attributes);
    }

    public interface IRxEntity
    {
        RxEntity Entities(Func<IEntityProperties, bool> func);

        RxEntity Entity(string entityId);
    }

    public interface IRxStateChange : IObservable<(EntityState Old, EntityState New)>
    {

    }
}
