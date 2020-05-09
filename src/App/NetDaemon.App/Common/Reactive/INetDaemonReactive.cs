using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

namespace JoySoftware.HomeAssistant.NetDaemon.Common.Reactive
{
    public interface IRxStateChange : IObservable<(EntityState Old, EntityState New)>
    {

    }

    public interface ICallService
    {
        void CallService(string domain, string service, dynamic? data);
    }

    public interface IRxEntity
    {
        RxEntity Entity(string entityId);
        RxEntity Entities(Func<IEntityProperties, bool> func);
    }

    /// <summary>
    ///     Implements the System.Reactive pattern for NetDaemon Apps
    /// </summary>
    public interface INetDaemonReactive : INetDaemonAppBase, ICallService, IRxEntity
    //IObservable<EntityState>

    {
        RxEntity Entity(string entityId);

        public IRxStateChange StateChanges { get; }

        EntityState? State(string entityId);
        IEnumerable<EntityState> States { get; }
    }
}
