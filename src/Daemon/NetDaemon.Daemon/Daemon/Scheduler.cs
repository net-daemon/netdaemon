using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common;

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
        private Task _schedulerTask;
        public Scheduler(IManageTime? timerManager = null)
        {
            _timeManager = timerManager ?? new TimeManager();

            _schedulerTask = Task.Run(SchedulerLoop, _cancelSource.Token);
        }

        /// <summary>
        ///     Time when task was completed, these probably wont be used more than in tests
        /// </summary>
        public DateTime CompletedTime { get; } = DateTime.MaxValue;

        /// <summary>
        ///     Calculated start time, these probably wont be used more than in tests
        /// </summary>
        public DateTime StartTime { get; } = DateTime.MinValue;

        public Task RunEvery(int millisecondsDelay, Func<Task> func)
        {
            return RunEvery(TimeSpan.FromMilliseconds(millisecondsDelay), func);
        }

        public Task RunEvery(TimeSpan timeSpan, Func<Task> func)
        {
            var stopWatch = new Stopwatch();

            var task = Task.Run(async () =>
            {
                while (!_cancelSource.IsCancellationRequested)
                {
                    stopWatch.Start();
                    await func.Invoke();
                    stopWatch.Stop();

                    // If less time spent in func that duration delay the remainder
                    if (timeSpan > stopWatch.Elapsed)
                    {
                        var diff = timeSpan.Subtract(stopWatch.Elapsed);
                        await _timeManager!.Delay(diff, _cancelSource.Token);
                    }
                    else
                    {
                        Console.WriteLine();
                    }
                    stopWatch.Reset(); 
                }
            }, _cancelSource.Token);

            ScheduleTask(task);

            return task;
        }

        public Task RunIn(int millisecondsDelay, Func<Task> func)
        {
            return RunIn(TimeSpan.FromMilliseconds(millisecondsDelay), func);
        }

        public Task RunIn(TimeSpan timeSpan, Func<Task> func)
        {
            var task = Task.Run(async () =>
            {
                await _timeManager!.Delay(timeSpan, _cancelSource.Token);
                await func.Invoke();
            }, _cancelSource.Token);

            ScheduleTask(task);

            return task;
        }


        public async Task Stop()
        {
            _cancelSource.Cancel();

            // Make sure we are waiting for the scheduler task as well
            _scheduledTasks[_schedulerTask.Id] = _schedulerTask;

            var taskResult = await Task.WhenAny(
                Task.WhenAll(_scheduledTasks.Values.ToArray()),  Task.Delay(1000));

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
                        await Task.Delay(DefaultSchedulerTimeout, _cancelSource.Token);
                    }
            }
            catch (OperationCanceledException)
            {
            }

        }

        private void ScheduleTask(Task addedTask)
        {
            _scheduledTasks[addedTask.Id] = addedTask;
        }
    }

    public class TimeManager : IManageTime
    {
        public DateTime Current { get; }

        public async Task Delay(TimeSpan timeSpan, CancellationToken token)
        {
            await Task.Delay(timeSpan, token);
        }
    }
}