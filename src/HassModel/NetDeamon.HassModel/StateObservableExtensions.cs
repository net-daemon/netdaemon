using System.Reactive.Concurrency;

namespace NetDaemon.HassModel;

/// <summary>
/// Provides extension methods for <see cref="IObservable&lt;StateChange&gt;"/>
/// </summary>
public static class StateObservableExtensions
{
    /// <summary>
    /// Ignores the OnCompletion event of the observable
    /// </summary>
    /// <remarks>
    /// In the case you do not want the action to be called when the observable is completed
    /// use this method before the use of other methods in the Observalble.
    /// For example if you use Throttle and you want the action call not to be
    /// run when you cancel the observable.
    /// For use-cases where you want to wait for a specific state for a specific time
    /// use <see cref="WhenStateIsFor"/> instead.
    /// </remarks>
    public static IObservable<T> IgnoreOnComplete<T>(this IObservable<T> source)
        => Observable.Create<T>(observer => source.Subscribe(observer.OnNext, observer.OnError));

    // public static IObservable<StateChange> IgnoreOnComplete(
    //     this IObservable<StateChange> observable)
    // {
    //     ArgumentNullException.ThrowIfNull(observable, nameof(observable));
    //
    //     var isCompleted = false;
    //
    //     return observable
    //         .Do(_ => {}, () => isCompleted = true)
    //         // But only when the new state matches the predicate we emit it
    //         .Where(_ => isCompleted == false);
    // }
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

        var isCompleted = false;

        return observable
            .Do(_ => {}, () => isCompleted = true)
            // Only process changes that start or stop matching the predicate
            .Where(e => predicate(e.Old) != predicate(e.New))

            // Both  will restart the timer
            .Throttle(timeSpan, scheduler)

            // But only when the new state matches the predicate we emit it
            .Where(e => predicate(e.New) && isCompleted == false);
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

        var isCompleted = false;

        return observable
            .Do(_ => {}, () => isCompleted = true)
            .Where(e => predicate(e.Old) != predicate(e.New))
            .Throttle(timeSpan, scheduler)
            .Where(e => predicate(e.New) && isCompleted == false);
    }
}
