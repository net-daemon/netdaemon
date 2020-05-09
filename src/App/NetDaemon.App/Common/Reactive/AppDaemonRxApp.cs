using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace JoySoftware.HomeAssistant.NetDaemon.Common.Reactive
{

    /// <summary>
    ///     Base class for using the Reactive paradigm for apps
    /// </summary>
    public abstract class NetDaemonRxApp : NetDaemonAppBase, INetDaemonReactive
    {
        ReactiveState? _reactiveState = null;

        public NetDaemonRxApp()
        {

        }

        public IRxStateChange StateChanges => _reactiveState;

        public IEnumerable<EntityState> States => 
            _daemon?.State ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

        public async override Task StartUpAsync(INetDaemon daemon)
        {
            await base.StartUpAsync(daemon);
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            _reactiveState = new ReactiveState(_daemon);
        }

        public EntityState? State(string entityId) => _daemon?.GetState(entityId);

        public RxEntity Entity(string entityId)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return new RxEntity(_daemon, new string[] { entityId });
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

        public void CallService(string domain, string service, dynamic data)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            _daemon.CallService(domain, service, data);
        }

        public void SetState(string entityId, dynamic state, dynamic? attributes= null)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            _daemon.SetState(entityId, state, attributes);
        }
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
