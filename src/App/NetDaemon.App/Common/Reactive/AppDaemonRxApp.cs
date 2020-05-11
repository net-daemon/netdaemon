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
    /// <summary>
    ///     Extension methods for Observables
    /// </summary>
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

        /// <inheritdoc/>
        public IRxEvent EventChanges =>
            _reactiveEvent ?? throw new ApplicationException("Application not initialized correctly");

        /// <inheritdoc/>
        public IRxStateChange StateAllChanges =>
            _reactiveState ?? throw new ApplicationException("Application not initialized correctly");

        /// <inheritdoc/>
        public IObservable<(EntityState Old, EntityState New)> StateChanges => _reactiveState.Where(e => e.New.State != e.Old.State);

        /// <inheritdoc/>
        public IEnumerable<EntityState> States =>
            _daemon?.State ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

        /// <inheritdoc/>
        public void CallService(string domain, string service, dynamic? data)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            _daemon.CallService(domain, service, data);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public RxEntity Entities(params string[] entityIds) => Entities((IEnumerable<string>)entityIds);

        /// <inheritdoc/>
        public RxEntity Entities(IEnumerable<string> entityIds)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return new RxEntity(_daemon, entityIds);
        }

        /// <inheritdoc/>
        public RxEntity Entity(string entityId)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return new RxEntity(_daemon, new string[] { entityId });
        }

        /// <inheritdoc/>
        public IObservable<long> RunDaily(string time)
        {
            DateTime timeOfDayToTrigger;

            if (!DateTime.TryParseExact(time, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out timeOfDayToTrigger))
            {
                throw new FormatException($"{time} is not a valid time for the current locale");
            }

            return Observable.Timer(timeOfDayToTrigger, TimeSpan.FromDays(1), TaskPoolScheduler.Default);
        }

        /// <inheritdoc/>
        public IObservable<long> RunEvery(TimeSpan timespan)
        {
            return Observable.Interval(timespan, TaskPoolScheduler.Default);
        }

        /// <inheritdoc/>
        public IObservable<long> RunIn(TimeSpan timespan)
        {
            return Observable.Timer(timespan, TaskPoolScheduler.Default);
        }

        /// <inheritdoc/>
        public void SetState(string entityId, dynamic state, dynamic? attributes = null)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            _daemon.SetState(entityId, state, attributes);
        }

        /// <inheritdoc/>
        public void RunScript(params string[] script)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

            foreach (var scriptName in script)
            {
                var name = scriptName;
                if (scriptName.Contains('.'))
                    name = scriptName[(scriptName.IndexOf('.') + 1)..];

                _daemon.CallService("script", name);
            }

        }

        /// <inheritdoc/>
        public async override Task StartUpAsync(INetDaemon daemon)
        {
            await base.StartUpAsync(daemon);
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            _reactiveState = new ReactiveState(daemon);
            _reactiveEvent = new ReactiveEvent(daemon);
        }

        /// <inheritdoc/>
        public EntityState? State(string entityId) => _daemon?.GetState(entityId);
    }

    /// <summary>
    ///     Implements Observable RxEvent
    /// </summary>
    public class ReactiveEvent : IRxEvent
    {
        private readonly INetDaemon _daemon;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="daemon">The NetDaemon host object</param>
        public ReactiveEvent(INetDaemon daemon)
        {
            _daemon = daemon;
        }

        /// <summary>
        ///     Implements IObservable ReactiveEvent
        /// </summary>
        /// <param name="observer">Observer</param>
        public IDisposable Subscribe(IObserver<RxEvent> observer)
        {
            return _daemon!.EventChanges.Subscribe(observer);
        }
    }

    /// <summary>
    ///     Implements the IObservable state changes
    /// </summary>
    public class ReactiveState : IRxStateChange
    {
        private readonly INetDaemon _daemon;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="daemon">The NetDaemon host object</param>
        public ReactiveState(INetDaemon daemon)
        {
            _daemon = daemon;
        }

        /// <summary>
        ///     Implements IObservable ReactivState
        /// </summary>
        /// <param name="observer">Observer</param>
        public IDisposable Subscribe(IObserver<(EntityState, EntityState)> observer)
        {
            return _daemon!.StateChanges.Subscribe(observer);
        }
    }

    /// <summary>
    ///     Represent an event from eventstream
    /// </summary>
    public class RxEvent
    {
        private readonly dynamic? _data;
        private readonly string? _domain;
        private readonly string _eventName;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="eventName">Event</param>
        /// <param name="domain">Domain</param>
        /// <param name="data">Data</param>
        public RxEvent(string eventName, string? domain, dynamic? data)
        {
            _eventName = eventName;
            _domain = domain;
            _data = data;
        }

        /// <summary>
        ///     Data from event
        /// </summary>
        public dynamic? Data => _data;

        /// <summary>
        ///     Domain (call service event)
        /// </summary>
        public dynamic? Domain => _domain;

        /// <summary>
        ///     The event being sent
        /// </summary>
        public string Event => _eventName;
    }
}