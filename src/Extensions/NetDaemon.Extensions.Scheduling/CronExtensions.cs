using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using Cronos;

namespace NetDaemon.Extensions.Scheduler;

/// <summary>
/// Adds Cron Scheduling capabilities to <see cref="System.Reactive.Concurrency.IScheduler"/>
/// </summary>
public static class CronExtensions
{
    /// <summary>
    /// Schedules an Action based on a Cron expression
    /// </summary>
    /// <param name="scheduler">IScheduler to use for this action</param>
    /// <param name="cron">Cron expression that describes the schedule</param>
    /// <param name="action">Callback to execute</param>
    /// <returns>Disposable object that allows the schedule to be cancelled</returns>
    public static IDisposable ScheduleCron(this IScheduler scheduler, string cron, Action action)
    {
        ArgumentNullException.ThrowIfNull(scheduler);

        // When this gets cancelled we only need to actually dispose of the most recent scheduled action
        // (there will only be one at a time) so we store that in a box we will pass down
        StrongBox<IDisposable?> disposableBox = new();
        RecursiveSchedule(scheduler, CronExpression.Parse(cron), action, disposableBox);

        // Dispose will Dispose the IDisposable in the box
        return Disposable.Create(()=> disposableBox.Value?.Dispose());
    }

    private static void RecursiveSchedule(IScheduler scheduler, CronExpression cronExpression, Action action, StrongBox<IDisposable?> disposableBox)
    {
        var next = cronExpression.GetNextOccurrence(scheduler.Now, TimeZoneInfo.Local);
        if (next.HasValue)
        {
            disposableBox.Value = scheduler.Schedule(next.Value, EcecuteAndReschedule);
        }

        void EcecuteAndReschedule()
        {
            try
            {
                action();
            }
            finally
            {
                RecursiveSchedule(scheduler, cronExpression, action, disposableBox);
            }
        }
    }
}