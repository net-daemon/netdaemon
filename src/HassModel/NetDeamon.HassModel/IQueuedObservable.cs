namespace NetDaemon.Infrastructure.ObservableHelpers;

/// <summary>
///     The observable that queues the events
/// </summary>
/// <remarks>
///     The default implementation will implement a queue and async behaviour
///     For testing a synchronous version will be added
/// </remarks>
public interface IQueuedObservable<T> : IObservable<T>, IAsyncDisposable
{
    /// <summary>
    ///     Initializes the observer with the inner observer
    /// </summary>
    void Initialize(IObservable<T> innerObservable);
}