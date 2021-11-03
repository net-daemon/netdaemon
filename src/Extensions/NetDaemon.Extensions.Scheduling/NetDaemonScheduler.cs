using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("NetDaemon.Extensions.Scheduling.Tests")]

namespace NetDaemon.Extensions.Scheduler
{
    /// <summary>
    ///     Provides scheduling capability to be injected into apps
    /// </summary>
    internal class NetDaemonScheduler : INetDaemonScheduler, IDisposable
    {
        private readonly CancellationTokenSource _cancelTimers;
        private bool disposedValue;

        private static ILoggerFactory DefaultLoggerFactory => LoggerFactory.Create(builder =>
        {
            builder
                .ClearProviders()
                .AddConsole();
        });

        private readonly ILogger _logger;
        private readonly IScheduler _reactiveScheduler;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="logger">Injected logger</param>
        /// <param name="reactiveScheduler">Used for unit testing the scheduler</param>
        public NetDaemonScheduler(ILogger<NetDaemonScheduler>? logger = null, IScheduler? reactiveScheduler = null)
        {
            _cancelTimers = new();
            _logger = logger ?? DefaultLoggerFactory.CreateLogger<NetDaemonScheduler>();
            _reactiveScheduler = reactiveScheduler ?? TaskPoolScheduler.Default;
        }

        /// <inheritdoc/>
        [SuppressMessage("", "CA1031")]
        public IDisposable RunEvery(TimeSpan timespan, Action action)
        {
            var result = new DisposableTimer(_cancelTimers.Token);

            Observable.Interval(timespan, _reactiveScheduler)
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
                            _logger.LogError(e, "Error in scheduled timer!");
                        }
                    },
                    _ => _logger.LogTrace("Exiting timer using trigger with span {span}",
                            timespan)
                    , result.Token);

            return result;
        }

        /// <inheritdoc/>
        [SuppressMessage("", "CA1031")]
        public IDisposable RunEvery(TimeSpan period, DateTimeOffset startTime, Action action)
        {
            var result = new DisposableTimer(_cancelTimers.Token);

            Observable.Timer(
                startTime,
                period,
                _reactiveScheduler)
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
                            _logger.LogError(e, "Error in scheduled timer!");
                        }
                    },
                    () => _logger.LogTrace("Exiting timer that was scheduled at {startTime} and every {period}",
                            startTime, period),
                    result.Token
                    );

            return result;
        }

        /// <inheritdoc/>
        [SuppressMessage("", "CA1031")] // In this case we want just to log the message
        public IDisposable RunIn(TimeSpan timespan, Action action)
        {
            var result = new DisposableTimer(_cancelTimers.Token);
            Observable.Timer(timespan, _reactiveScheduler)
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
                            _logger.LogWarning(te, "Timeout Exception thrown in please catch it in user code");
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Error in scheduled timer!");
                        }
                    },
                    () => _logger.LogTrace("Exiting scheduled timer...")
                    , result.Token);
            return result;
        }
        /// <inheritdoc/>
        [SuppressMessage("", "CA1031")]
        public IDisposable RunAt(DateTimeOffset timeOffset, Action action)
        {
            var result = new DisposableTimer(_cancelTimers.Token);

            Observable.Timer(
                timeOffset,
                _reactiveScheduler)
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
                            _logger.LogError(e, "Error in scheduled timer!");
                        }
                    },
                    () => _logger.LogTrace("Exiting timer that was scheduled at {timeOffset}",
                            timeOffset),
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