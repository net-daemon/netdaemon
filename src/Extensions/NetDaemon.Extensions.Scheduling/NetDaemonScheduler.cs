using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace NetDaemon.Extensions.Scheduler
{
    /// <summary>
    ///     Provides scheduling capability to be injected into apps
    /// </summary>
    public class NetDaemonScheduler : INetDaemonScheduler, IDisposable
    {
        private readonly CancellationTokenSource _cancelTimers;
        private bool disposedValue;

        private static ILoggerFactory DefaultLoggerFactory => LoggerFactory.Create(builder =>
        {
            builder
                .ClearProviders()
                .AddConsole();
        });

        private ILogger _log { get; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="loggerFactory">Injected logger factory</param>
        public NetDaemonScheduler(ILoggerFactory? loggerFactory = null)
        {
            _cancelTimers = new();
            loggerFactory = loggerFactory ?? DefaultLoggerFactory;
            _log = loggerFactory.CreateLogger<NetDaemonScheduler>();
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

        /// <summary>
        ///     Creates an observable intervall
        /// </summary>
        /// <param name="timespan">Time span for intervall</param>
        /// <param name="action">The action to call</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        internal virtual IDisposable CreateObservableIntervall(TimeSpan timespan, Action action)
        {
            var result = new DisposableTimer(_cancelTimers.Token);

            Observable.Interval(timespan, TaskPoolScheduler.Default)
                .Subscribe(
                    _ =>
                    {
                        try
                        {
                            action();
                        }
                        catch (OperationCanceledException)
                        {
                            // Do nothing
                        }
                        catch (Exception e)
                        {
                            _log.LogError(e, "Error in scheduled timer!");
                        }
                    },
                    _ => _log.LogTrace("Exiting timer using trigger with span {span}",
                            timespan)
                    , result.Token);

            return result;
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
                // It is not due until the next hour
                timeOfDayToTrigger = timeOfDayToTrigger.AddHours(1);
            }

            return CreateObservableTimer(timeOfDayToTrigger, TimeSpan.FromHours(1), action);
        }

        /// <inheritdoc/>
        public IDisposable RunEveryMinute(short second, Action action)
        {
            var now = DateTime.Now;
            var startTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, second).AddMinutes(1);

            return CreateObservableTimer(startTime, TimeSpan.FromMinutes(1), action);
        }

        /// <inheritdoc/>
        [SuppressMessage("", "CA1031")] // In this case we want just to log the message
        public IDisposable RunIn(TimeSpan timespan, Action action)
        {
            var result = new DisposableTimer(_cancelTimers.Token);
            Observable.Timer(timespan, TaskPoolScheduler.Default)
                .Subscribe(
                    _ =>
                    {
                        try
                        {
                            action();
                        }
                        catch (OperationCanceledException)
                        {
                            // Do nothing
                        }
                        catch (TimeoutException te)
                        {
                            // Ignore
                            _log.LogWarning(te, "Timeout Exception thrown in please catch it in user code");
                        }
                        catch (Exception e)
                        {
                            _log.LogError(e, "Error in scheduled timer!");
                        }
                    },
                    () => _log.LogTrace("Exiting scheduled timer...")
                    , result.Token);
            return result;
        }

        /// <inheritdoc/>
        public void CancelAllTimers()
        {
            _cancelTimers.Cancel();
        }

        [SuppressMessage("", "CA1031")]
        internal virtual IDisposable CreateObservableTimer(DateTime timeOfDayToTrigger, TimeSpan interval, Action action)
        {
            var result = new DisposableTimer(_cancelTimers.Token);

            Observable.Timer(
                timeOfDayToTrigger,
                interval,
                TaskPoolScheduler.Default)
                .Subscribe(
                    _ =>
                    {
                        try
                        {
                            action();
                        }
                        catch (OperationCanceledException)
                        {
                            // Do nothing
                        }
                        catch (Exception e)
                        {
                            _log.LogError(e, "Error in scheduled timer!");
                        }
                    },
                    () => _log.LogTrace("Exiting timer using trigger {trigger} and span {span}",
                            timeOfDayToTrigger, interval),
                    result.Token
                    );

            return result;
        }

        /// <summary>
        ///     Dispose the runnting timers correctly by cancel them
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _cancelTimers.Cancel();
                    _cancelTimers.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        ///     Implements Dispose pattern
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}