using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;

namespace NetDaemon.Daemon
{
    /// <summary>
    ///     Interface to be able to mock the time
    /// </summary>
    public interface IManageTime
    {
        DateTime Current { get; }

        Task Delay(TimeSpan timeSpan, CancellationToken token);
    }

    public class Scheduler : IScheduler
    {
        private const int DefaultSchedulerTimeout = 100;

        /// <summary>
        ///     Used to cancel all running tasks
        /// </summary>
        private CancellationTokenSource _cancelSource = new();

        private readonly ConcurrentDictionary<int, Task> _scheduledTasks
                = new();

        private readonly IManageTime? _timeManager;
        private readonly ILogger<Scheduler> _logger;
        private readonly Task _schedulerTask;

        public Scheduler(IManageTime? timerManager = null, ILoggerFactory? loggerFactory = null)
        {
            _timeManager = timerManager ?? new TimeManager();
            loggerFactory ??= DefaultLoggerFactory;
            _logger = loggerFactory.CreateLogger<Scheduler>();
            _schedulerTask = Task.Run(SchedulerLoop, _cancelSource.Token);
        }

        private static ILoggerFactory DefaultLoggerFactory => LoggerFactory.Create(builder =>
                                {
                                    builder
                                        .ClearProviders()
                                        .AddConsole();
                                });

        /// <inheritdoc/>
        public ISchedulerResult RunEvery(int millisecondsDelay, Func<Task> func)
        {
            return RunEvery(TimeSpan.FromMilliseconds(millisecondsDelay), func);
        }

        /// <inheritdoc/>
        public ISchedulerResult RunEvery(TimeSpan timeSpan, Func<Task> func)
        {
            var cancelSource = new CancellationTokenSource();
            var task = RunEveryInternalAsync(timeSpan, func, cancelSource.Token);

            ScheduleTask(task);

            return new SchedulerResult(task, cancelSource);
        }

        private async Task RunEveryInternalAsync(TimeSpan timeSpan, Func<Task> func, CancellationToken token)
        {
            using CancellationTokenSource linkedCts =
                CancellationTokenSource.CreateLinkedTokenSource(_cancelSource.Token, token);

            var stopWatch = new Stopwatch();
            while (!linkedCts.IsCancellationRequested)
            {
                try
                {
                    stopWatch.Start();
                    await func.Invoke().ConfigureAwait(false);
                    stopWatch.Stop();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Unhandled exception invoking scheduled function");
                }
                // If less time spent in func that duration delay the remainder
                if (timeSpan > stopWatch.Elapsed)
                {
                    var diff = timeSpan.Subtract(stopWatch.Elapsed);
                    _logger.LogTrace("RunEvery, Time: {time}, Span: {timeSpan},  Delay {diff}", _timeManager!.Current, timeSpan, diff);
                    await _timeManager!.Delay(diff, linkedCts.Token).ConfigureAwait(false);
                }
                else
                {
                    Console.WriteLine();
                }
                stopWatch.Reset();
            }
        }

        internal TimeSpan CalculateDailyTimeBetweenNowAndTargetTime(DateTime targetTime)
        {
            var now = _timeManager!.Current;
            var timeToTrigger = new DateTime(now.Year, now.Month, now.Day, targetTime.Hour, targetTime.Minute, targetTime.Second);

            if (now > timeToTrigger)
            {
                timeToTrigger = timeToTrigger.AddDays(1);
            }
            return timeToTrigger.Subtract(now);
        }

        internal TimeSpan CalculateEveryMinuteTimeBetweenNowAndTargetTime(short second)
        {
            var now = _timeManager!.Current;
            if (now.Second >= second)
            {
                return TimeSpan.FromSeconds(60 - now.Second + second);
            }
            return TimeSpan.FromSeconds(second - now.Second);
        }

        /// <inheritdoc/>
        public ISchedulerResult RunDaily(string time, Func<Task> func) => RunDaily(time, null, func);

        /// <inheritdoc/>
        public ISchedulerResult RunDaily(string time, IEnumerable<DayOfWeek>? runOnDays, Func<Task> func)
        {
            var cancelSource = new CancellationTokenSource();

            if (!DateTime.TryParseExact(time, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime timeOfDayToTrigger))
            {
                throw new FormatException($"{time} is not a valid time for the current locale");
            }

            var task = RunDailyInternalAsync(timeOfDayToTrigger, runOnDays, func, cancelSource.Token);
            ScheduleTask(task);

            return new SchedulerResult(task, cancelSource);
        }

        private async Task RunDailyInternalAsync(DateTime timeOfDayToTrigger, IEnumerable<DayOfWeek>? runOnDays, Func<Task> func, CancellationToken token)
        {
            using CancellationTokenSource linkedCts =
                CancellationTokenSource.CreateLinkedTokenSource(_cancelSource.Token, token);

            while (!linkedCts.IsCancellationRequested)
            {
                var diff = CalculateDailyTimeBetweenNowAndTargetTime(timeOfDayToTrigger);
                _logger.LogTrace("RunDaily, Time: {time}, parsed time: {timeOfDayToTrigger},  Delay {diff}", _timeManager!.Current, timeOfDayToTrigger, diff);
                await _timeManager!.Delay(diff, linkedCts.Token).ConfigureAwait(false);

                if (runOnDays != null)
                {
                    if (runOnDays.Contains(_timeManager!.Current.DayOfWeek))
                    {
                        try
                        {
                            _logger.LogTrace("RunDaily, Time: Invoke function {time}, parsed time: {timeOfDayToTrigger}", _timeManager!.Current, timeOfDayToTrigger);
                            await func.Invoke().ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            _logger.LogWarning(e, "Unhandled exception invoking scheduled function");
                        }
                    }
                    else
                    {
                        _logger.LogTrace("RunDaily, Time: {time}, parsed time: {timeOfDayToTrigger}, Not run, due to dayofweek", _timeManager!.Current, timeOfDayToTrigger);
                    }
                }
                else
                {
                    try
                    {
                        _logger.LogTrace("RunDaily, Time: Invoke function {time}, parsed time: {timeOfDayToTrigger}", _timeManager!.Current, timeOfDayToTrigger);
                        // No constraints on day of week
                        await func.Invoke().ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, "Unhandled exception invoking scheduled function");
                    }
                }
            }
        }

        /// <inheritdoc/>
        public ISchedulerResult RunEveryMinute(short second, Func<Task> func)
        {
            var cancelSource = new CancellationTokenSource();
            var task = RunEveryMinuteInternalAsync(second, func, cancelSource.Token);

            ScheduleTask(task);

            return new SchedulerResult(task, cancelSource);
        }

        private async Task RunEveryMinuteInternalAsync(short second, Func<Task> func, CancellationToken token)
        {
            using CancellationTokenSource linkedCts =
                CancellationTokenSource.CreateLinkedTokenSource(_cancelSource.Token, token);

            while (!linkedCts.IsCancellationRequested)
            {
                var diff = CalculateEveryMinuteTimeBetweenNowAndTargetTime(second);
                _logger.LogTrace("RunEveryMinute, Delay {diff}", diff);
                await _timeManager!.Delay(diff, linkedCts.Token).ConfigureAwait(false);
                try
                {
                    await func.Invoke().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Unhandled exception invoking scheduled function");
                }
            }
        }

        /// <inheritdoc/>
        public ISchedulerResult RunIn(int millisecondsDelay, Func<Task> func) => RunIn(TimeSpan.FromMilliseconds(millisecondsDelay), func);

        /// <inheritdoc/>
        public ISchedulerResult RunIn(TimeSpan timeSpan, Func<Task> func)
        {
            var cancelSource = new CancellationTokenSource();
            var task = InternalRunInAsync(timeSpan, func, cancelSource.Token);
            ScheduleTask(task);

            return new SchedulerResult(task, cancelSource);
        }

        private async Task InternalRunInAsync(TimeSpan timeSpan, Func<Task> func, CancellationToken token)
        {
            using CancellationTokenSource linkedCts =
                CancellationTokenSource.CreateLinkedTokenSource(_cancelSource.Token, token);

            _logger.LogTrace("RunIn, Delay {timeSpan}", timeSpan);
            await _timeManager!.Delay(timeSpan, linkedCts.Token).ConfigureAwait(false);
            try
            {
                await func.Invoke().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Unhandled exception invoking scheduled function");
            }
        }

        /// <summary>
        ///     Stops the scheduler
        /// </summary>
        public async Task Stop()
        {
            _cancelSource.Cancel();

            // Make sure we are waiting for the scheduler task as well
            _scheduledTasks[_schedulerTask.Id] = _schedulerTask;

            var taskResult = await Task.WhenAny(
                Task.WhenAll(_scheduledTasks.Values.ToArray()), Task.Delay(1000)).ConfigureAwait(false);

            if (_scheduledTasks.Values.Any(n => !n.IsCompleted))
            {
                // Todo: Some kind of logging have to be done here to tell user which task caused timeout
                throw new ApplicationException("Failed to cancel all tasks");
            }
        }

        /// <summary>
        ///     Restarts the scheduler.
        ///     Existing schedules are cancelled and the scheduler remains usable.
        /// </summary>
        public async Task Restart()
        {
            await Stop().ConfigureAwait(false);
            _cancelSource = new CancellationTokenSource();
        }

        private async Task SchedulerLoop()
        {
            try
            {
                while (!_cancelSource.Token.IsCancellationRequested)
                {
                    if (!_scheduledTasks.IsEmpty)
                    {
                        // Make sure we do cleaning and handle new task every 100 ms
                        ScheduleTask(Task.Delay(DefaultSchedulerTimeout,
                            _cancelSource.Token)); // Todo: Work out a proper timing

                        var task = await Task.WhenAny(_scheduledTasks.Values.ToArray())
                            .ConfigureAwait(false);

                        // Todo: handle errors here if not removing
                        _scheduledTasks.TryRemove(task.Id, out _);
                    }
                    else
                    {
                        await Task.Delay(DefaultSchedulerTimeout, _cancelSource.Token).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {// Normal, just ignore
            }
        }

        private void ScheduleTask(Task addedTask)
        {
            _scheduledTasks[addedTask.Id] = addedTask;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (!_cancelSource.IsCancellationRequested)
                    await Stop().ConfigureAwait(false);
            }
            catch // Ignore errors in cleanup
            {
            }
        }
    }

    /// <summary>
    ///     Abstract time functions to be able to mock
    /// </summary>
    public class TimeManager : IManageTime
    {
        /// <summary>
        ///     Returns current local time
        /// </summary>
        /// <value></value>
        public DateTime Current => DateTime.Now;

        /// <summary>
        ///     Delays a given timespan time
        /// </summary>
        /// <param name="timeSpan">Timespan to delay</param>
        /// <param name="token">Cancelation token to cancel delay</param>
        public async Task Delay(TimeSpan timeSpan, CancellationToken token)
        {
            var expectedDelay = timeSpan;
            // Use in while loop incase delay a time less than expected
            while (!token.IsCancellationRequested)
            {
                var timeBefore = Current;
                await Task.Delay(expectedDelay, token).ConfigureAwait(false);
                var elapsedTime = Current.Subtract(timeBefore);

                if (elapsedTime >= expectedDelay)
                    return; // Expected

                // Compensate for the time left plus add on millisecond
                expectedDelay = expectedDelay - elapsedTime + TimeSpan.FromMilliseconds(1);
            }
        }
    }

    public class SchedulerResult : ISchedulerResult
    {
        public SchedulerResult(Task scheduledTask, CancellationTokenSource cancelSource)
        {
            Task = scheduledTask;
            CancelSource = cancelSource;
        }

        public Task Task { get; }
        public CancellationTokenSource CancelSource { get; }
    }
}