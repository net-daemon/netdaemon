using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NetDaemon.Common
{
    /// <summary>
    ///     Interface for scheduler actions
    /// </summary>
    [Obsolete("You are using V1 of API and it is deprecated, next release it will be moved.Please replace it wiht V2 NetDaemonRxApp", false)]
    public interface IScheduler : IAsyncDisposable
    {
        /// <summary>
        ///     Run daily tasks
        /// </summary>
        /// <param name="time">The time in the format HH:mm:ss</param>
        /// <param name="func">The action to run</param>
        /// <returns></returns>
        ISchedulerResult RunDaily(string time, Func<Task> func);

        /// <summary>
        ///     Run daily tasks
        /// </summary>
        /// <param name="time">The time in the format HH:mm:ss</param>
        /// <param name="runOnDays">A list of days the scheduler will run on</param>
        /// <param name="func">The action to run</param>
        /// <returns></returns>
        ISchedulerResult RunDaily(string time, IEnumerable<DayOfWeek>? runOnDays, Func<Task> func);

        /// <summary>
        ///     Run function every milliseconds
        /// </summary>
        /// <param name="millisecondsDelay">Number of milliseconds</param>
        /// <param name="func">The function to run</param>
        ISchedulerResult RunEvery(int millisecondsDelay, Func<Task> func);

        /// <summary>
        ///     Run function every time span
        /// </summary>
        /// <param name="timeSpan">Timespan between runs</param>
        /// <param name="func">The function to run</param>
        ISchedulerResult RunEvery(TimeSpan timeSpan, Func<Task> func);

        /// <summary>
        ///      Run task every minute at given second
        /// </summary>
        /// <param name="second">The second in a minute to start (0-59)</param>
        /// <param name="func">The task to run</param>
        /// <remarks>
        ///     It is safe to supress the task since it is handled internally in the scheduler
        /// </remarks>
        ISchedulerResult RunEveryMinute(short second, Func<Task> func);

        /// <summary>
        ///     Run in milliseconds delay
        /// </summary>
        /// <param name="millisecondsDelay">Number of milliseconds before run</param>
        /// <param name="func">The function to run</param>
        ISchedulerResult RunIn(int millisecondsDelay, Func<Task> func);

        /// <summary>
        ///     Run in function in time span
        /// </summary>
        /// <param name="timeSpan">Timespan time before run function</param>
        /// <param name="func">The function to run</param>
        ISchedulerResult RunIn(TimeSpan timeSpan, Func<Task> func);
    }

    /// <summary>
    ///     Scheduler result lets you manage scheduled tasks like check completion, cancel the tasks etc.
    /// </summary>
    public interface ISchedulerResult
    {
        /// <summary>
        ///     Use to cancel any scheduled execution
        /// </summary>
        CancellationTokenSource CancelSource { get; }

        /// <summary>
        ///     Current running task
        /// </summary>
        Task Task { get; }
    }
}