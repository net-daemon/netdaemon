using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{

    /// <summary>
    ///     Interface for scheduler actions
    /// </summary>
    public interface IScheduler
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

    }
}
