using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
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

        private CancellationTokenSource _cancelTimers = new CancellationTokenSource();


        /// <inheritdoc/>
        public IRxEvent EventChanges =>
            _reactiveEvent ?? throw new ApplicationException("Application not initialized correctly (EventChanges)");

        private EventObservable? _eventObservables;

        /// <summary>
        ///     Returns the observables events implementation of AppDaemonRxApps
        /// </summary>
        public ObservableBase<RxEvent> EventChangesObservable => _eventObservables!;

        private StateChangeObservable? _stateObservables;

        /// <summary>
        ///     Returns the observables states implementation of AppDaemonRxApps
        /// </summary>
        public ObservableBase<(EntityState, EntityState)> StateChangesObservable => _stateObservables!;

        /// <inheritdoc/>
        public IObservable<(EntityState Old, EntityState New)> StateAllChanges =>
            _reactiveState ?? throw new ApplicationException("Application not initialized correctly (StateAllChanges>");

        /// <inheritdoc/>
        public IObservable<(EntityState Old, EntityState New)> StateChanges => _reactiveState.Where(e => e.New?.State != e.Old?.State);


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

                return new RxEntity(this, x.Select(n => n.EntityId).ToArray());
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
            return new RxEntity(this, entityIds);
        }

        /// <inheritdoc/>
        public RxEntity Entity(string entityId)
        {
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return new RxEntity(this, new string[] { entityId });
        }

        /// <inheritdoc/>
        public IDisposable RunEveryMinute(short second, Action action)
        {
            var now = DateTime.Now;
            var startTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute + 1, second);

            return CreateObservableTimer(startTime, TimeSpan.FromMinutes(1), action);
        }

        /// <inheritdoc/>
        public IDisposable RunDaily(string time, Action action)
        {
            DateTime parsedTime;

            if (!DateTime.TryParseExact(time, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTime))
            {
                throw new FormatException($"{time} is not a valid time for the current locale");
            }
            var now = DateTime.Now;
            var timeOfDayToTrigger = new DateTime(
                now.Year,
                now.Month,
                now.Day,
                parsedTime.Hour,
                parsedTime.Minute,
                parsedTime.Second
            );

            if (now > timeOfDayToTrigger)
                // It is not due until tomorrow
                timeOfDayToTrigger = timeOfDayToTrigger.AddDays(1);

            return CreateObservableTimer(timeOfDayToTrigger, TimeSpan.FromDays(1), action);
        }

        /// <inheritdoc/>
        public IDisposable RunEveryHour(string time, Action action)
        {
            DateTime parsedTime;
            time = $"{DateTime.Now.Hour:D2}:{time}";


            if (!DateTime.TryParseExact(time, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTime))
            {
                throw new FormatException($"{time} is not a valid time for the current locale");
            }

            var now = DateTime.Now;
            var timeOfDayToTrigger = new DateTime(
                now.Year,
                now.Month,
                now.Day,
                now.Hour,
                parsedTime.Minute,
                parsedTime.Second
            );

            if (now > timeOfDayToTrigger)
                // It is not due until tomorrow
                timeOfDayToTrigger = timeOfDayToTrigger.AddHours(1);

            return CreateObservableTimer(timeOfDayToTrigger, TimeSpan.FromHours(1), action);

        }

        private IDisposable CreateObservableTimer(DateTime timeOfDayToTrigger, TimeSpan interval, Action action)
        {
            var result = new DisposableTimerResult(_cancelTimers.Token);

            Observable.Timer(
                timeOfDayToTrigger,
                interval,
                TaskPoolScheduler.Default)
                .Subscribe(
                    s =>
                    {
                        try
                        {
                            if (this.IsEnabled)
                                action();
                        }
                        catch (OperationCanceledException)
                        {
                            // Do nothing
                        }
                        catch (Exception e)
                        {
                            LogError(e, "Error, RunDaily APP: {app}", Id ?? "unknown");
                        }

                    },
                    ex =>
                    {
                        LogError(ex, "Error, RunDaily_ex APP: {app}", Id ?? "unknown");
                    },
                    () => Log("Exiting timer for app {app}, {trigger}:{span}",
                            Id!, timeOfDayToTrigger, interval),
                    result.Token
                     );

            return result;
        }


        /// <inheritdoc/>
        public IDisposable RunEvery(TimeSpan timespan, Action action)
        {
            var result = new DisposableTimerResult(_cancelTimers.Token);

            Observable.Interval(timespan, TaskPoolScheduler.Default)
                .Subscribe(
                    s =>
                    {
                        try
                        {
                            if (this.IsEnabled)
                                action();
                        }
                        catch (OperationCanceledException)
                        {
                            // Do nothing
                        }
                        catch (Exception e)
                        {
                            LogError(e, "Error, RunEvery APP: {app}", Id ?? "unknown");
                        }

                    },
                    ex =>
                    {
                        LogError(ex, "Error, RunEvery_ex APP: {app}", Id ?? "unknown");
                    },
                    () => Log("Exiting RunEvery for app {app}, {trigger}:{span}", Id!, timespan)
                    , result.Token);

            return result;
        }

        /// <inheritdoc/>
        public IDisposable RunIn(TimeSpan timespan, Action action)
        {
            var result = new DisposableTimerResult(_cancelTimers.Token);
            Observable.Timer(timespan, TaskPoolScheduler.Default)
                .Subscribe(
                    s =>
                    {
                        try
                        {
                            if (this.IsEnabled)
                                action();
                        }
                        catch (OperationCanceledException)
                        {
                            // Do nothing
                        }
                        catch (Exception e)
                        {
                            LogError(e, "Error, RunIn APP: {app}", Id ?? "unknown");
                        }

                    },
                    ex =>
                    {
                        LogError(ex, "Error, RunIn_ex APP: {app}", Id ?? "unknown");
                    },
                    () => Log("Exiting RunIn for app {app}, {trigger}:{span}", Id!, timespan)
                    , result.Token);
            return result;
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
            _ = Logger ?? throw new NullReferenceException("Logger can not be null!");

            _eventObservables = new EventObservable(Logger, this);
            _stateObservables = new StateChangeObservable(Logger, this);
            _reactiveState = new ReactiveState(this);
            _reactiveEvent = new ReactiveEvent(this);
        }

        /// <inheritdoc/>
        public EntityState? State(string entityId) => _daemon?.GetState(entityId);

        /// <summary>
        ///     Implements the async dispose pattern
        /// </summary>
        public async override ValueTask DisposeAsync()
        {
            // To end timers
            _cancelTimers.Cancel();

            if (_eventObservables is object)
                _eventObservables!.Clear();

            if (_stateObservables is object)
                _stateObservables!.Clear();

            _eventObservables = null;
            _stateObservables = null;

            // Make sure we release all references so the apps can be
            // unloaded correctly
            _reactiveEvent = null;
            _reactiveState = null;



            await base.DisposeAsync().ConfigureAwait(false);
            Log("RxApp {app} is Disposed", Id!);
        }

    }

    /// <summary>
    ///     Implements Observable RxEvent
    /// </summary>
    public class ReactiveEvent : IRxEvent
    {
        private readonly NetDaemonRxApp _daemonRxApp;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="daemon">The NetDaemon host object</param>
        public ReactiveEvent(NetDaemonRxApp daemon)
        {
            _daemonRxApp = daemon;
        }

        /// <summary>
        ///     Implements IObservable ReactiveEvent
        /// </summary>
        /// <param name="observer">Observer</param>
        public IDisposable Subscribe(IObserver<RxEvent> observer)
        {
            return _daemonRxApp!.EventChangesObservable.Subscribe(observer);
        }
    }

    /// <summary>
    ///     Implements the IObservable state changes
    /// </summary>
    public class ReactiveState : IObservable<(EntityState Old, EntityState New)>
    {
        private readonly NetDaemonRxApp _daemonRxApp;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="daemon">The NetDaemon host object</param>
        public ReactiveState(NetDaemonRxApp daemon)
        {
            _daemonRxApp = daemon;
        }

        /// <summary>
        ///     Implements IObservable ReactivState
        /// </summary>
        /// <param name="observer">Observer</param>
        public IDisposable Subscribe(IObserver<(EntityState, EntityState)> observer)
        {
            return _daemonRxApp.StateChangesObservable.Subscribe(observer);
        }
    }


    /// <summary>
    ///     Represent an event from eventstream
    /// </summary>
    public struct RxEvent
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


    /// <summary>
    ///     Implements the observable interface for state changes
    /// </summary>
    public class StateChangeObservable : ObservableBase<(EntityState, EntityState)>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="app">App being tracked</param>
        public StateChangeObservable(ILogger logger, INetDaemonAppBase app)
            : base(logger, app)
        { }
    }


    #region IObservable<T> implementation

    /// <summary>
    ///     Implements the observable interface for state changes
    /// </summary>
    public class ObservableBase<T> : IObservable<T>
    {
        private readonly ConcurrentDictionary<IObserver<T>, IObserver<T>>
            _observersTuples = new ConcurrentDictionary<IObserver<T>, IObserver<T>>();
        private readonly ILogger _logger;
        private readonly INetDaemonAppBase _app;

        /// <summary>
        ///     List of current observers for a app
        /// </summary>
        public IEnumerable<IObserver<T>> Observers => _observersTuples.Values;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="logger">A ILogger instance</param>
        /// <param name="app">App being tracked</param>
        public ObservableBase(ILogger logger, INetDaemonAppBase app)
        {
            _logger = logger;
            _app = app;
        }

        /// <summary>
        ///     Clear all observers
        /// </summary>
        public void Clear()
        {
            foreach (var eventObservable in _observersTuples)
            {
                try
                {
                    eventObservable.Value.OnCompleted();
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Error complete the observables for app {app}", _app.Id);
                }
            }
            _observersTuples.Clear();
        }

        /// <summary>
        ///     Subscribes to observable
        /// </summary>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (!_observersTuples.ContainsKey(observer))
                _observersTuples.TryAdd(observer, observer);

            return new UnsubscriberObservable<T>(_observersTuples, observer);
        }
        private class UnsubscriberObservable<X> : IDisposable
        {
            private readonly IObserver<X> _observer;
            private readonly ConcurrentDictionary<IObserver<X>, IObserver<X>> _observers;

            public UnsubscriberObservable(
                ConcurrentDictionary<IObserver<X>, IObserver<X>> observers, IObserver<X> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (_observer is object)
                {
                    _observers.TryRemove(_observer, out _);
                }
                // System.Console.WriteLine($"Subscribers:{_observers.Count}");
            }
        }
    }

    #endregion

    /// <summary>
    ///     Implements a IDisposable to cancel timers
    /// </summary>
    public class DisposableTimerResult : IDisposable
    {
        private readonly CancellationTokenSource _internalToken;
        private readonly CancellationTokenSource _combinedToken;

        /// <summary>
        ///     Token to use as cancellation
        /// </summary>
        public CancellationToken Token => _combinedToken.Token;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="token">App cancellation token to combine</param>
        public DisposableTimerResult(CancellationToken token)
        {
            _internalToken = new CancellationTokenSource();
            _combinedToken = CancellationTokenSource.CreateLinkedTokenSource(_internalToken.Token, token);
        }

        /// <summary>
        ///     Disposes and cancel timerrs
        /// </summary>
        public void Dispose()
        {
            _internalToken.Cancel();
        }
    }


    /// <summary>
    ///     Implements the observable interface for event changes
    /// </summary>
    public class EventObservable : ObservableBase<RxEvent>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="app">App being tracked</param>
        public EventObservable(ILogger logger, INetDaemonAppBase app)
            : base(logger, app)
        { }
    }
}