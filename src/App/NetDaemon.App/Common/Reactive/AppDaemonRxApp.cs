using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.Common.Exceptions;
using NetDaemon.Common.Fluent;

// For mocking
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace NetDaemon.Common.Reactive
{
    /// <summary>
    ///     Base class for using the Reactive paradigm for apps
    /// </summary>
    public abstract class NetDaemonRxApp : NetDaemonAppBase, INetDaemonReactive
    {
        private readonly CancellationTokenSource _cancelTimers;
        private EventObservable? _eventObservables;
        private StateChangeObservable? _stateObservables;
        private bool _isDisposed;

        /// <summary>
        ///     Default constructor
        /// </summary>
        protected NetDaemonRxApp()
        {
            _cancelTimers = new();
            StateAllChanges = new ReactiveState(this);
            EventChanges = new ReactiveEvent(this);
            _isDisposed = false;
        }

        /// <inheritdoc/>
        public IRxEvent EventChanges { get; }

        /// <summary>
        ///     Returns the observables events implementation of AppDaemonRxApps
        /// </summary>
        public ObservableBase<RxEvent> EventChangesObservable => _eventObservables!;

        /// <inheritdoc/>
        public IObservable<(EntityState Old, EntityState New)> StateAllChanges { get; }

        /// <inheritdoc/>
        public IObservable<(EntityState Old, EntityState New)> StateChanges => StateAllChanges.Where(e => e.New?.State != e.Old?.State);

        /// <summary>
        ///     Returns the observables states implementation of AppDaemonRxApps
        /// </summary>
        public ObservableBase<(EntityState, EntityState)> StateChangesObservable => _stateObservables!;

        /// <inheritdoc/>
        public IEnumerable<EntityState> States =>
            Daemon?.State ?? new List<EntityState>();

        /// <inheritdoc/>
        public void CallService(string domain, string service, dynamic? data)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            Daemon.CallService(domain, service, data);
        }

        /// <summary>
        ///     Implements the async dispose pattern
        /// </summary>
        public async override ValueTask DisposeAsync()
        {
            lock (_cancelTimers)
            {
                if (_isDisposed)
                    return;
                _isDisposed = true;
            }

            LogDebug("RxApp {app} is being Disposes", Id!);
            // To end timers
            _cancelTimers?.Cancel();

            if (_eventObservables is not null)
                _eventObservables!.Clear();

            if (_stateObservables is not null)
                _stateObservables!.Clear();

            _cancelTimers?.Dispose();

            await base.DisposeAsync().ConfigureAwait(false);
            LogDebug("RxApp {app} is Disposed", Id!);
        }

        /// <inheritdoc/>
        public RxEntity Entities(Func<IEntityProperties, bool> func)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");

            try
            {
                IEnumerable<IEntityProperties> x = Daemon.State.Where(func);

                return new RxEntity(this, x.Select(n => n.EntityId).ToArray());
            }
            catch (Exception e)
            {
                Daemon.Logger.LogDebug(e, "Failed to select entities func in app {appId}", Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public RxEntity Entities(params string[] entityIds) => Entities((IEnumerable<string>)entityIds);

        /// <inheritdoc/>
        public RxEntity Entities(IEnumerable<string> entityIds)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return new RxEntity(this, entityIds);
        }

        /// <inheritdoc/>
        public RxEntity Entity(string entityId)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return new RxEntity(this, new string[] { entityId });
        }

        /// <inheritdoc/>
        public T? GetData<T>(string id) where T : class
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return Daemon.GetDataAsync<T>(id).Result;
        }

        /// <inheritdoc/>
        public IDisposable RunDaily(string time, Action action)
        {
            if (!DateTime.TryParseExact(time, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime))
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
            {
                // It is not due until tomorrow
                timeOfDayToTrigger = timeOfDayToTrigger.AddDays(1);
            }

            return CreateObservableTimer(timeOfDayToTrigger, TimeSpan.FromDays(1), action);
        }

        /// <inheritdoc/>
        public IDisposable RunEvery(TimeSpan timespan, Action action)
        {
            return CreateObservableIntervall(timespan, action);
        }

        /// <inheritdoc/>
        public IDisposable RunEveryHour(string time, Action action)
        {
            time = $"{DateTime.Now.Hour:D2}:{time}";

            if (!DateTime.TryParseExact(time, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime))
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
            {
                // It is not due until tomorrow
                timeOfDayToTrigger = timeOfDayToTrigger.AddHours(1);
            }

            return CreateObservableTimer(timeOfDayToTrigger, TimeSpan.FromHours(1), action);
        }

        /// <inheritdoc/>
        public IDisposable RunEveryMinute(short second, Action action)
        {
            var now = DateTime.Now;
            var startTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute + 1, second);

            return CreateObservableTimer(startTime, TimeSpan.FromMinutes(1), action);
        }

        /// <inheritdoc/>
        [SuppressMessage("", "CA1031")] // In this case we want just to log the message
        public IDisposable RunIn(TimeSpan timespan, Action action)
        {
            var result = new DisposableTimerResult(_cancelTimers.Token);
            Observable.Timer(timespan, TaskPoolScheduler.Default)
                .Subscribe(
                    _ =>
                    {
                        try
                        {
                            if (IsEnabled)
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
                    () => LogTrace("Exiting RunIn for app {app}, {trigger}:{span}", Id!, timespan)
                    , result.Token);
            return result;
        }

        /// <inheritdoc/>
        public void RunScript(params string[] script)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");

            foreach (var scriptName in script)
            {
                var name = scriptName;
                if (scriptName.Contains('.', StringComparison.InvariantCultureIgnoreCase))
                    name = scriptName[(scriptName.IndexOf('.', StringComparison.InvariantCultureIgnoreCase) + 1)..];

                Daemon.CallService("script", name);
            }
        }

        /// <inheritdoc/>
        public void SaveData<T>(string id, T data)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            Daemon.SaveDataAsync(id, data).Wait();
        }

        /// <inheritdoc/>
        public void SetState(string entityId, dynamic state, dynamic? attributes = null)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            Daemon.SetState(entityId, state, attributes);
        }

        /// <inheritdoc/>
        public async override Task StartUpAsync(INetDaemon daemon)
        {
            await base.StartUpAsync(daemon).ConfigureAwait(false);
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            _ = Logger ?? throw new NetDaemonNullReferenceException("Logger can not be null!");

            _eventObservables = new EventObservable(Logger, this);
            _stateObservables = new StateChangeObservable(Logger, this);
        }

        /// <inheritdoc/>
        public EntityState? State(string entityId) => Daemon?.GetState(entityId);

        /// <summary>
        ///     Creates an observable intervall
        /// </summary>
        /// <param name="timespan">Time span for intervall</param>
        /// <param name="action">The action to call</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        internal virtual IDisposable CreateObservableIntervall(TimeSpan timespan, Action action)
        {
            var result = new DisposableTimerResult(_cancelTimers.Token);
            RuntimeInfo.NextScheduledEvent = DateTime.Now + timespan;
            UpdateRuntimeInformation();

            Observable.Interval(timespan, TaskPoolScheduler.Default)
                .Subscribe(
                    _ =>
                    {
                        try
                        {
                            if (IsEnabled)
                            {
                                action();
                                RuntimeInfo.NextScheduledEvent = DateTime.Now + timespan;
                                UpdateRuntimeInformation();
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // Do nothing
                        }
                        catch (Exception e)
                        {
                            LogError(e, "Error, ObservableIntervall APP: {app}", Id ?? "unknown");
                        }
                    },
                    ex => LogTrace(ex, "Exiting ObservableIntervall for app {app}, {trigger}:{span}", Id!, timespan)
                    , result.Token);

            return result;
        }

        /// <summary>
        ///     Creates a observable timer that are tracked for errors
        /// </summary>
        /// <param name="timeOfDayToTrigger">When to start the timer</param>
        /// <param name="interval">The intervall</param>
        /// <param name="action">Action to call each intervall</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]

        internal virtual IDisposable CreateObservableTimer(DateTime timeOfDayToTrigger, TimeSpan interval, Action action)
        {
            var result = new DisposableTimerResult(_cancelTimers.Token);

            RuntimeInfo.NextScheduledEvent = timeOfDayToTrigger;
            UpdateRuntimeInformation();

            Observable.Timer(
                timeOfDayToTrigger,
                interval,
                TaskPoolScheduler.Default)
                .Subscribe(
                    _ =>
                    {
                        try
                        {
                            if (IsEnabled)
                            {
                                action();
                                RuntimeInfo.NextScheduledEvent = DateTime.Now + interval;
                                UpdateRuntimeInformation();
                            }
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
                    () => Log("Exiting timer for app {app}, {trigger}:{span}",
                            Id!, timeOfDayToTrigger, interval),
                    result.Token
                    );

            return result;
        }
    }
}