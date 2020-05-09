using System;
using System.Collections.Generic;

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

        void SetState(string entityId, dynamic state, dynamic? attributes);

        EntityState? State(string entityId);
    }

    public interface IRxEntity
    {
        RxEntity Entities(Func<IEntityProperties, bool> func);

        RxEntity Entities(IEnumerable<string> entityIds);

        RxEntity Entities(params string[] entityIds);

        RxEntity Entity(string entityId);
    }

    public interface IRxStateChange : IObservable<(EntityState Old, EntityState New)>
    {
    }
}