using System;

namespace NetDaemon.Extensions.Scheduler;

/// <summary>
///     Scheduler interface
/// </summary>
public interface INetDaemonScheduler
{
    /// <summary>
    ///     Schedules an action every (timespan)
    /// </summary>
    /// <param name="period">The period to schedule</param>
    /// <param name="action">Action to run</param>
    IDisposable RunEvery(TimeSpan period, Action action);

    /// <summary>
    ///     Schedules an action every (timespan)
    /// </summary>
    /// <param name="period">The period to schedule</param>
    /// <param name="startTime">The time to start the schedule</param>
    /// <param name="action">Action to run</param>
    IDisposable RunEvery(TimeSpan period, DateTimeOffset startTime, Action action);

    /// <summary>
    ///     Delays execution of an action (timespan) time
    /// </summary>
    /// <param name="timespan">Timespan to delay</param>
    /// <param name="action">Action to run</param>
    IDisposable RunIn(TimeSpan timespan, Action action);

    /// <summary>
    ///     Runs an action at a specific time
    /// </summary>
    /// <param name="timeOffset">Absolute time to run the action</param>
    /// <param name="action">Action to run</param>
    IDisposable RunAt(DateTimeOffset timeOffset, Action action);

    /// <summary>
    ///     The current time of the scheduler
    /// </summary>
    DateTimeOffset Now { get; }
}