using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Common.Reactive
{
    public static class ObservableExtensionMethods
    {
        /// <summary>
        ///     Is same for timespan time
        /// </summary>
        /// <param name="observable"></param>
        /// <param name="span"></param>
        /// <returns></returns>
        public static IObservable<(EntityState Old, EntityState New)> NDSameStateFor(this IObservable<(EntityState Old, EntityState New)> observable, TimeSpan span)
        {
            return observable.Throttle(span);
        }
    }

    /// <summary>
    ///     Base class for using the Reactive paradigm for apps
    /// </summary>
    public abstract class NetDaemonRxApp : NetDaemonAppBase, INetDaemonReactive
    {
        private ReactiveEvent? _reactiveEvent = null;
        private ReactiveState? _reactiveState = null;
        public NetDaemonRxApp()
        {
        }

        public IRxEvent EventChanges => _reactiveEvent;
        public IRxStateChange StateAllChanges => _reactiveState;
        public IObservable<(EntityState Old, EntityState New)> StateChanges => _reactiveState.Where(e => e.New.State != e.Old.State);
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

        public IObservable<long> RunDaily(string time)
        {
            DateTime timeOfDayToTrigger;

            if (!DateTime.TryParseExact(time, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out timeOfDayToTrigger))
            {
                throw new FormatException($"{time} is not a valid time for the current locale");
            }

            return Observable.Timer(timeOfDayToTrigger, TimeSpan.FromDays(1), TaskPoolScheduler.Default);
        }

        public IObservable<long> RunEvery(TimeSpan timespan)
        {
            return Observable.Interval(timespan, TaskPoolScheduler.Default);
        }

        public IObservable<long> RunIn(TimeSpan timespan)
        {
            return Observable.Timer(timespan, TaskPoolScheduler.Default);
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
            _reactiveEvent = new ReactiveEvent(_daemon);
        }

        public EntityState? State(string entityId) => _daemon?.GetState(entityId);
    }

    public class ReactiveEvent : IRxEvent
    {
        private readonly INetDaemon _daemon;

        public ReactiveEvent(INetDaemon daemon)
        {
            _daemon = daemon;
        }

        public IDisposable Subscribe(IObserver<RxEvent> observer)
        {
            return _daemon!.EventChanges.Subscribe(observer);
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
            return _daemon!.StateChanges.Subscribe(observer);
        }
    }
    public class RxEvent
    {
        private readonly dynamic? _data;
        private readonly string? _domain;
        private readonly string _eventName;
        public RxEvent(string eventName, string? domain, dynamic? data)
        {
            _eventName = eventName;
            _domain = domain;
            _data = data;
        }

        public dynamic? Data => _data;
        public dynamic? Domain => _domain;
        public string Event => _eventName;
    }
}