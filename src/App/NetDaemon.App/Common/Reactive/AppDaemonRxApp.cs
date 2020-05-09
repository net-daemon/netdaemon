using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Common.Reactive
{
    /// <summary>
    ///     Base class for using the Reactive paradigm for apps
    /// </summary>
    public abstract class NetDaemonRxApp : NetDaemonAppBase, INetDaemonReactive
    {
        private ReactiveState? _reactiveState = null;

        public NetDaemonRxApp()
        {
        }

        public IRxStateChange StateChanges => _reactiveState;

        public IEnumerable<EntityState> States =>
            _daemon?.State ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

        public void CallService(string domain, string service, dynamic data)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            _daemon.CallService(domain, service, data);
        }

        public RxEntity Entities(Func<IEntityProperties, bool> func)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

            try
            {
                IEnumerable<IEntityProperties> x = _daemon.State.Where(func);

                return new RxEntity(_daemon, x.Select(n => n.EntityId).ToArray());
            }
            catch (Exception e)
            {
                _daemon.Logger.LogDebug(e, "Failed to select entities func in app {appId}", Id);
                throw;
            }
        }

        public RxEntity Entities(params string[] entityIds) => Entities((IEnumerable<string>)entityIds);

        public RxEntity Entities(IEnumerable<string> entityIds)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return new RxEntity(_daemon, entityIds);
        }

        public RxEntity Entity(string entityId)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return new RxEntity(_daemon, new string[] { entityId });
        }

        public void SetState(string entityId, dynamic state, dynamic? attributes = null)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            _daemon.SetState(entityId, state, attributes);
        }

        public async override Task StartUpAsync(INetDaemon daemon)
        {
            await base.StartUpAsync(daemon);
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            _reactiveState = new ReactiveState(_daemon);
        }

        public EntityState? State(string entityId) => _daemon?.GetState(entityId);
    }

    public class ReactiveState : IRxStateChange
    {
        private readonly INetDaemon _daemon;

        public ReactiveState(INetDaemon daemon)
        {
            _daemon = daemon;
        }

        public IDisposable Subscribe(IObserver<(EntityState, EntityState)> observer)
        {
            return _daemon!.Subscribe(observer);
        }
    }
}