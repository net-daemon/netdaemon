using System;

namespace NetDaemon.Extensions.Scheduler
{
    /// <summary>
    ///     Scheduler interface
    /// </summary>
    public interface INetDaemonScheduler
    {
        /// <summary>
        ///     Cancel all scheduled timers on this instance
        /// </summary>
        void CancelAllTimers();

        /// <summary>
        ///     Run daily at a specific time
        /// </summary>
        /// <param name="time">The time in "hh:mm:ss" format</param>
        /// <param name="action">Action to run</param>
        IDisposable RunDaily(string time, Action action);

        /// <summary>
        ///     Shedules an action every (timespan)
        /// </summary>
        /// <param name="timespan">The timespan to schedule</param>
        /// <param name="action">Action to run</param>
        IDisposable RunEvery(TimeSpan timespan, Action action);

        /// <summary>
        ///     Shedules an action every (timespan)
        /// </summary>
        /// <param name="time">The time in "mm:ss" format</param>
        /// <param name="action">Action to run</param>
        IDisposable RunEveryHour(string time, Action action);

        /// <summary>
        ///     Shedules an action every (timespan)
        /// </summary>
        /// <param name="second">The timespan to schedule</param>
        /// <param name="action">Action to run</param>
        IDisposable RunEveryMinute(short second, Action action);

        /// <summary>
        ///     Delays excecution of an action (timespan) time
        /// </summary>
        /// <param name="timespan">Timespan to delay</param>
        /// <param name="action">Action to run</param>
        IDisposable RunIn(TimeSpan timespan, Action action);
    }
}