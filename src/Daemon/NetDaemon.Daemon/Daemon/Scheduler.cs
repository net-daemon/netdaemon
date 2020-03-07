using JoySoftware.HomeAssistant.NetDaemon.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Daemon
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
        private readonly CancellationTokenSource _cancelSource = new CancellationTokenSource();

        private readonly ConcurrentDictionary<int, Task> _scheduledTasks
                    = new ConcurrentDictionary<int, Task>();

        private readonly IManageTime? _timeManager;
        private readonly ILogger<Scheduler> _logger;
        private Task _schedulerTask;

        public Scheduler(IManageTime? timerManager = null, ILoggerFactory? loggerFactory = null)
        {
            _timeManager = timerManager ?? new TimeManager();
            loggerFactory = loggerFactory ?? DefaultLoggerFactory;
            _logger = loggerFactory.CreateLogger<Scheduler>();
            _schedulerTask = Task.Run(SchedulerLoop, _cancelSource.Token);
        }

        private static ILoggerFactory DefaultLoggerFactory => LoggerFactory.Create(builder =>
                                {
                                    builder
                                        .ClearProviders()
                                        .AddConsole();
                                });

        /// <summary>
        ///     Time when task was completed, these probably wont be used more than in tests
        /// </summary>
        public DateTime CompletedTime { get; } = DateTime.MaxValue;

        /// <summary>
        ///     Calculated start time, these probably wont be used more than in tests
        /// </summary>
        public DateTime StartTime { get; } = DateTime.MinValue;

        /// <inheritdoc/>
        public void RunEvery(int millisecondsDelay, Func<Task> func) => RunEveryAsync(millisecondsDelay, func);

        /// <inheritdoc/>
        public Task RunEveryAsync(int millisecondsDelay, Func<Task> func)
        {
            return RunEveryAsync(TimeSpan.FromMilliseconds(millisecondsDelay), func);
        }

        /// <inheritdoc/>
        public void RunEvery(TimeSpan timeSpan, Func<Task> func) => RunEveryAsync(timeSpan, func);

        /// <inheritdoc/>
        public Task RunEveryAsync(TimeSpan timeSpan, Func<Task> func)
        {
            var task = RunEveryInternalAsync(timeSpan, func);
            // var stopWatch = new Stopwatch();

            // var task = Task.Run(async () =>
            // {
            //     while (!_cancelSource.IsCancellationRequested)
            //     {
            //         stopWatch.Start();
            //         await func.Invoke();
            //         stopWatch.Stop();

            //         // If less time spent in func that duration delay the remainder
            //         if (timeSpan > stopWatch.Elapsed)
            //         {
            //             var diff = timeSpan.Subtract(stopWatch.Elapsed);
            //             await _timeManager!.Delay(diff, _cancelSource.Token);
            //         }
            //         else
            //         {
            //             Console.WriteLine();
            //         }
            //         stopWatch.Reset();
            //     }
            // }, _cancelSource.Token);

            ScheduleTask(task);

            return task;
        }

        private async Task RunEveryInternalAsync(TimeSpan timeSpan, Func<Task> func)
        {
            var stopWatch = new Stopwatch();
            while (!_cancelSource.IsCancellationRequested)
            {
                stopWatch.Start();
                await func.Invoke().ConfigureAwait(false);
                stopWatch.Stop();

                // If less time spent in func that duration delay the remainder
                if (timeSpan > stopWatch.Elapsed)
                {
                    var diff = timeSpan.Subtract(stopWatch.Elapsed);
                    _logger.LogTrace($"RunEvery, Time: {_timeManager!.Current}, Span: {timeSpan},  Delay {diff}");
                    await _timeManager!.Delay(diff, _cancelSource.Token).ConfigureAwait(false);
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
        public void RunDaily(string time, Func<Task> func) => RunDailyAsync(time, func);

        /// <inheritdoc/>
        public void RunDaily(string time, IEnumerable<DayOfWeek>? runOnDays, Func<Task> func) =>
            RunDailyAsync(time, runOnDays, func);

        /// <inheritdoc/>
        public Task RunDailyAsync(string time, Func<Task> func) => RunDailyAsync(time, null, func);

        /// <inheritdoc/>
        public Task RunDailyAsync(string time, IEnumerable<DayOfWeek>? runOnDays, Func<Task> func)
        {
            DateTime timeOfDayToTrigger;
            if (!DateTime.TryParseExact(time, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out timeOfDayToTrigger))
            {
                throw new FormatException($"{time} is not a valid time for the current locale");
            }

            var task = RunDailyInternalAsync(timeOfDayToTrigger, runOnDays, func);
            ScheduleTask(task);

            return task;
        }

        private async Task RunDailyInternalAsync(DateTime timeOfDayToTrigger, IEnumerable<DayOfWeek>? runOnDays, Func<Task> func)
        {
            while (!_cancelSource.IsCancellationRequested)
            {
                var diff = CalculateDailyTimeBetweenNowAndTargetTime(timeOfDayToTrigger);
                _logger.LogTrace($"RunDaily, Time: {_timeManager!.Current}, parsed time: {timeOfDayToTrigger},  Delay {diff}");
                await _timeManager!.Delay(diff, _cancelSource.Token).ConfigureAwait(false);

                if (runOnDays != null)
                {
                    if (runOnDays.Contains(_timeManager!.Current.DayOfWeek))
                    {
                        _logger.LogTrace($"RunDaily, Time: Invoke function {_timeManager!.Current}, parsed time: {timeOfDayToTrigger}");
                        await func.Invoke().ConfigureAwait(false);
                    }
                    else
                        _logger.LogTrace($"RunDaily, Time: {_timeManager!.Current}, parsed time: {timeOfDayToTrigger}, Not run, due to dayofweek");
                }
                else
                {
                    _logger.LogTrace($"RunDaily, Time: Invoke function {_timeManager!.Current}, parsed time: {timeOfDayToTrigger}");
                    // No constraints on day of week
                    await func.Invoke().ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc/>
        public void RunEveryMinute(short second, Func<Task> func) => RunEveryMinuteAsync(second, func);

        /// <inheritdoc/>
        public Task RunEveryMinuteAsync(short second, Func<Task> func)
        {
            //     var task = Task.Run(async () =>
            //   {
            //       while (!_cancelSource.IsCancellationRequested)
            //       {
            //           var now = _timeManager?.Current;
            //           var diff = CalculateEveryMinuteTimeBetweenNowAndTargetTime(second);
            //           _logger.LogTrace($"RunEveryMinute, Delay {diff}");
            //           await _timeManager!.Delay(diff, _cancelSource.Token).ConfigureAwait(false);
            //           await func.Invoke().ConfigureAwait(false);

            //       }
            //   }, _cancelSource.Token);
            var task = RunEveryMinuteInternalAsync(second, func);

            ScheduleTask(task);

            return task;
        }

        private async Task RunEveryMinuteInternalAsync(short second, Func<Task> func)
        {
            while (!_cancelSource.IsCancellationRequested)
            {
                var now = _timeManager?.Current;
                var diff = CalculateEveryMinuteTimeBetweenNowAndTargetTime(second);
                _logger.LogTrace($"RunEveryMinute, Delay {diff}");
                await _timeManager!.Delay(diff, _cancelSource.Token).ConfigureAwait(false);
                await func.Invoke().ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public void RunIn(int millisecondsDelay, Func<Task> func) => RunInAsync(millisecondsDelay, func);

        /// <inheritdoc/>
        public Task RunInAsync(int millisecondsDelay, Func<Task> func)
        {
            return RunInAsync(TimeSpan.FromMilliseconds(millisecondsDelay), func);
        }

        /// <inheritdoc/>
        public void RunIn(TimeSpan timeSpan, Func<Task> func) => RunInAsync(timeSpan, func);

        /// <inheritdoc/>
        public Task RunInAsync(TimeSpan timeSpan, Func<Task> func)
        {
            // var task = Task.Run(async () =>
            // {
            //     _logger.LogTrace($"RunIn, Delay {timeSpan}");
            //     await _timeManager!.Delay(timeSpan, _cancelSource.Token).ConfigureAwait(false);
            //     await func.Invoke().ConfigureAwait(false);
            // }, _cancelSource.Token);

            var task = InternalRunInAsync(timeSpan, func);
            ScheduleTask(task);

            return task;
        }

        private async Task InternalRunInAsync(TimeSpan timeSpan, Func<Task> func)
        {
            _logger.LogTrace($"RunIn, Delay {timeSpan}");
            await _timeManager!.Delay(timeSpan, _cancelSource.Token).ConfigureAwait(false);
            await func.Invoke().ConfigureAwait(false);
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

            if (_scheduledTasks.Values.Count(n => n.IsCompleted == false) > 0)
                // Todo: Some kind of logging have to be done here to tell user which task caused timeout
                throw new ApplicationException("Failed to cancel all tasks");
        }

        private async Task SchedulerLoop()
        {
            try
            {
                while (!_cancelSource.Token.IsCancellationRequested)
                    if (_scheduledTasks.Count > 0)
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
}