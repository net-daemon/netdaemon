using System.Reactive;
using System.Threading.Channels;

namespace NetDaemon.HassModel.Internal;

/// <summary>
/// Wraps an Observable and queues all events
/// </summary>
/// <remarks>
/// Each QueuedObservable that is created from a single source IObservable
/// creates one subscription on that source IObservable.
/// All receive events are directly consumed and added to a queue specific to this QueuedObservable
///
/// This allows the events from each QueuedObservable to be consumed independently
/// without slow consumers preventing the events to be processed from other queues
/// The QueuedObservable preserves the ordering of events
/// When the QueuedObservable is Disposed it will stop the subscription from its source Observable
/// and wait until all events in the queue are processed
/// </remarks>
internal sealed class QueuedObservable<T> : IObservable<T>, IAsyncDisposable
{
    private const int Capacity = 1024;
    private readonly ILogger _logger;
    private volatile bool _isDisposed;

    private readonly Channel<T> _channel = Channel.CreateBounded<T>(Capacity);
    private readonly Subject<T> _subject = new();
    private readonly Task _processChannelTask;
    private readonly IDisposable _subscription;

    public QueuedObservable(IObservable<T> innerObservable, ILogger logger)
    {
        _logger = logger;

        // Subscribe to the innerObservable and write all events to the channel
        _subscription = CreateSubscription(innerObservable, logger);

        // Start processing the channel on a thread pool thread
        _processChannelTask = Task.Run(async () => await ProcessChannelAsync().ConfigureAwait(false));
    }

    private IDisposable CreateSubscription(IObservable<T> innerObservable, ILogger logger)
    {
        return innerObservable.Subscribe(
            onNext: e =>
            {
                if (_channel.Reader.Count > Capacity * 0.9)
                {
                    logger.LogWarning("EventQueue is nearing max Capacity of {Capacity}. Make sure event handlers do not block or events might be dropped", Capacity);
                }
                if (!_channel.Writer.TryWrite(e))
                {
                    logger.LogError("EventQueue exceeds max Capacity of {Capacity}. Events are being dropped. Make sure event handlers do not block", Capacity);
                }
            },
            onError: e => _channel.Writer.TryComplete(e),
            onCompleted: () => _channel.Writer.TryComplete()
        );
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        return _subject.Subscribe(CatchObserverExceptions(observer, _logger));
    }

    [SuppressMessage("", "CA1031")]
    private async Task ProcessChannelAsync()
    {
        // Now we read the stream from the channel and forward it to the subject
        try
        {
            await foreach (var @event in _channel.Reader.ReadAllAsync(CancellationToken.None).ConfigureAwait(false))
            {
                _subject.OnNext(@event);
            }

            // If we get here, the _channel was completed, either because the innerObservable completed, or this QueuedObservable was disposed
            _subject.OnCompleted();
        }
        catch (Exception e)
        {
            // This catch will catch Exceptions from the channel.Writer.TryComplete(e)
            // so it will essentially forward onError from the innerObservable to OnError of the subscribers
            _subject.OnError(e);
        }
    }

    private static IObserver<T> CatchObserverExceptions(IObserver<T> observer, ILogger logger)
    {
        return Observer.Create<T>(
            onNext: e =>
            {
                try
                {
                    observer.OnNext(e);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Exception in observer.OnNext");
                }
            },
            onError: e =>
            {
                try
                {
                    observer.OnError(e);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Exception in observer.OnError");
                }
            },
            onCompleted: () =>
            {
                try
                {
                    observer.OnCompleted();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Exception in observer.OnCompleted");
                }
            });
    }

    public async ValueTask DisposeAsync()
    {
        if(_isDisposed) return;
        _isDisposed = true;

        // Unsubscribe from inner observable so no new events will be sent to the channel
        _subscription.Dispose();

        // Mark the channel complete, this will break the processing loop when the last event in the channel is processed
        _channel.Writer.TryComplete();

        // Now await for the processing loop to be ready so we know all pending events are processed
        await _processChannelTask.ConfigureAwait(false);

        _subject.Dispose();
    }
}
