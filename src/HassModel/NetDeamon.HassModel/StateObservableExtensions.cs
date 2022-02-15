﻿using System.Reactive.Concurrency;

namespace NetDaemon.HassModel;

/// <summary>
/// Provides extension methods for <see cref="IObservable&lt;StateChange&gt;"/>
/// </summary>
public static class StateObservableExtensions
{
    /// <summary>
    /// Waits for an EntitySte to match a predicate for the specified time
    /// </summary>
    public static IObservable<StateChange> WhenStateIsFor(
        this IObservable<StateChange> observable, 
        Func<EntityState?, bool> predicate,
        TimeSpan timeSpan,
        IScheduler? scheduler = null)
    
        => observable
            .Where(e => predicate(e.Old) != predicate(e.New))
            .Throttle(timeSpan, scheduler ?? Scheduler.Default)
            .Where(e => predicate(e.New));
    
    /// <summary>
    /// Waits for an EntitySte to match a predicate for the specified time
    /// </summary>
    public static IObservable<StateChange<TEntity, TEntityState>> WhenStateIsFor<TEntity, TEntityState>(
        this IObservable<StateChange<TEntity, TEntityState>> observable, 
        Func<TEntityState?, bool> predicate, 
        TimeSpan timeSpan,
        IScheduler? scheduler = null)
        where TEntity : Entity
        where TEntityState : EntityState 

        => observable
            .Where(e => predicate(e.Old) != predicate(e.New))
            .Throttle(timeSpan, scheduler ?? Scheduler.Default)
            .Where(e => predicate(e.New));
}