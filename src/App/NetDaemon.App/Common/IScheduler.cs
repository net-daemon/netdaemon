using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{

    /// <summary>
    ///     Interface for scheduler actions
    /// </summary>
    public interface IScheduler : IAsyncDisposable
    {
        /// <summary>
        ///     Run function every milliseconds
        /// </summary>
        /// <param name="millisecondsDelay">Number of milliseconds</param>
        /// <param name="func">The function to run</param>
        Task RunEveryAsync(int millisecondsDelay, Func<Task> func);

        /// <summary>
        ///     Run function every time span
        /// </summary>
        /// <param name="timeSpan">Timespan between runs</param>
        /// <param name="func">The function to run</param>
        Task RunEveryAsync(TimeSpan timeSpan, Func<Task> func);

        /// <summary>
        ///     Run in milliseconds delay
        /// </summary>
        /// <param name="millisecondsDelay">Number of milliseconds before run</param>
        /// <param name="func">The function to run</param>
        Task RunInAsync(int millisecondsDelay, Func<Task> func);

        /// <summary>
        ///     Run in function in time span
        /// </summary>
        /// <param name="timeSpan">Timespan time before run function</param>
        /// <param name="func">The function to run</param>
        Task RunInAsync(TimeSpan timeSpan, Func<Task> func);

        /// <summary>
        ///     Run function every milliseconds
        /// </summary>
        /// <param name="millisecondsDelay">Number of milliseconds</param>
        /// <param name="func">The function to run</param>
        void RunEvery(int millisecondsDelay, Func<Task> func);

        /// <summary>
        ///     Run function every time span
        /// </summary>
        /// <param name="timeSpan">Timespan between runs</param>
        /// <param name="func">The function to run</param>
        void RunEvery(TimeSpan timeSpan, Func<Task> func);

        /// <summary>
        ///     Run in milliseconds delay
        /// </summary>
        /// <param name="millisecondsDelay">Number of milliseconds before run</param>
        /// <param name="func">The function to run</param>
        void RunIn(int millisecondsDelay, Func<Task> func);

        /// <summary>
        ///     Run in function in time span
        /// </summary>
        /// <param name="timeSpan">Timespan time before run function</param>
        /// <param name="func">The function to run</param>
        void RunIn(TimeSpan timeSpan, Func<Task> func);

        /// <summary>
        ///     Run daily tasks
        /// </summary>
        /// <param name="time">The time in the format HH:mm:ss</param>
        /// <param name="func">The action to run</param>
        /// <returns></returns>
        void RunDaily(string time, Func<Task> func);

        /// <summary>
        ///     Run daily tasks
        /// </summary>
        /// <param name="time">The time in the format HH:mm:ss</param>
        /// <param name="func">The action to run</param>
        /// <returns></returns>
        Task RunDailyAsync(string time, Func<Task> func);

        /// <summary>
        ///     Run daily tasks
        /// </summary>
        /// <param name="time">The time in the format HH:mm:ss</param>
        /// <param name="func">The action to run</param>
        /// <param name="runOnDays">A list of days the scheduler will run on</param>
        /// <returns></returns>
        void RunDaily(string time, IEnumerable<DayOfWeek>? runOnDays, Func<Task> func);

        /// <summary>
        ///     Run daily tasks
        /// </summary>
        /// <param name="time">The time in the format HH:mm:ss</param>
        /// <param name="func">The action to run</param>
        /// <param name="runOnDays">A list of days the scheduler will run on</param>
        /// <returns></returns>
        Task RunDailyAsync(string time, IEnumerable<DayOfWeek>? runOnDays, Func<Task> func);

        /// <summary>
        ///      Run task every minute at given second
        /// </summary>
        /// <param name="second">The second in a minute to start (0-59)</param>
        /// <param name="func">The task to run</param>
        /// <remarks>
        ///     It is safe to supress the task since it is handled internally in the scheduler
        /// </remarks>
        void RunEveryMinute(short second, Func<Task> func) => RunEveryMinuteAsync(second, func);

        /// <summary>
        ///      Run task every minute at given second
        /// </summary>
        /// <param name="second">The second in a minute to start (0-59)</param>
        /// <param name="func">The task to run</param>
        /// <remarks>
        ///     It is safe to supress the task since it is handled internally in the scheduler
        /// </remarks>
        Task RunEveryMinuteAsync(short second, Func<Task> func);

    }
}
