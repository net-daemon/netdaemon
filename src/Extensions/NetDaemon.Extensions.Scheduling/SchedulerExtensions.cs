using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;

namespace NetDaemon.Extensions.Scheduler;

/// <summary>
/// Extension Methods for IScheduler
/// </summary>
public static class SchedulerExtensions
{
    /// <summary>
    ///     Schedules an action every (timespan)
    /// </summary>
    /// <param name="scheduler">The Scheduler to use</param>
    /// <param name="period">The period to schedule</param>
    /// <param name="startTime">The time to start the schedule</param>
    /// <param name="action">Action to run</param>
    public static IDisposable RunEvery(this IScheduler scheduler, TimeSpan period, DateTimeOffset startTime, Action action)
    {
        return Observable.Timer(startTime, period, scheduler).Subscribe(_ => action());
    }

    internal static IScheduler WrapWithLogger(this IScheduler scheduler, ILogger logger) =>
        scheduler.Catch<Exception>(e =>
        {
            logger.LogError(e, "Error in scheduled task");
            return true; // Marks the exception as handled
        });
}