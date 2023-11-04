using System.Reactive.Concurrency;

namespace NetDaemon.HassModel;

/// <summary>
/// Provides extension methods for <see cref="IObservable&lt;StateChange&gt;"/>
/// </summary>
public static class StateObservableExtensions
{
    /// <summary>
    /// Waits for an EntityState to match a predicate for the specified time
    /// </summary>
    [Obsolete("Use the overload with IScheduler instead")]
    public static IObservable<StateChange> WhenStateIsFor(
        this IObservable<StateChange> observable,
        Func<EntityState?, bool> predicate,
        TimeSpan timeSpan)
        => observable.WhenStateIsFor(predicate, timeSpan, Scheduler.Default);

    /// <summary>
    /// Waits for an EntityState to match a predicate for the specified time
    /// </summary>
    [Obsolete("Use the overload with IScheduler instead")]
    public static IObservable<StateChange<TEntity, TEntityState>> WhenStateIsFor<TEntity, TEntityState>(
        this IObservable<StateChange<TEntity, TEntityState>> observable,
        Func<TEntityState?, bool> predicate,
        TimeSpan timeSpan)
        where TEntity : Entity
        where TEntityState : EntityState
        => observable.WhenStateIsFor(predicate, timeSpan, Scheduler.Default);

    /// <summary>
    /// Waits for an EntityState to match a predicate for the specified time
    /// </summary>
    public static IObservable<StateChange> WhenStateIsFor(
        this IObservable<StateChange> observable,
        Func<EntityState?, bool> predicate,
        TimeSpan timeSpan,
        IScheduler scheduler)
    {
        ArgumentNullException.ThrowIfNull(observable, nameof(observable));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentNullException.ThrowIfNull(scheduler, nameof(scheduler));

        return observable
            // Only process changes that start or stop matching the predicate
            .Where(e => predicate(e.Old) != predicate(e.New))

            // Both  will restart the timer
            .Throttle(timeSpan, scheduler)

            // But only when the new state matches the predicate we emit it
            .Where(e => predicate(e.New));
    }

    /// <summary>
    /// Waits for an EntityState to match a predicate for the specified time
    /// </summary>
    public static IObservable<StateChange<TEntity, TEntityState>> WhenStateIsFor<TEntity, TEntityState>(
        this IObservable<StateChange<TEntity, TEntityState>> observable,
        Func<TEntityState?, bool> predicate,
        TimeSpan timeSpan,
        IScheduler scheduler)
        where TEntity : Entity
        where TEntityState : EntityState
    {
        ArgumentNullException.ThrowIfNull(observable, nameof(observable));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentNullException.ThrowIfNull(scheduler, nameof(scheduler));

        return observable
            .Where(e => predicate(e.Old) != predicate(e.New))
            .Throttle(timeSpan, scheduler)
            .Where(e => predicate(e.New));
    }
}
