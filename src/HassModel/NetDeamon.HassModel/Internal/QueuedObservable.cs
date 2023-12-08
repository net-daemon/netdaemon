using System.Threading.Channels;
using NetDaemon.Infrastructure.ObservableHelpers;

namespace NetDaemon.HassModel.Internal;

/// <summary>
///     Wraps an Observable and queues all events in tasks
/// </summary>
/// <remarks>
///     QueueObservable allows asynchronous management of IObservable
///     that guarantee the order in where subscription gets the messages.
///     To avoid complicated async behaviour in tests a synchronous version
///     of IQueuedObservable is used.
/// </remarks>
internal sealed class QueuedObservable<T> : IQueuedObservable<T>
{
    private bool _isDisposed;
    private readonly ILogger<IHaContext> _logger;

    private readonly Channel<T> _queue = Channel.CreateBounded<T>(100);
    private readonly Subject<T> _subject = new();
    private readonly CancellationTokenSource _tokenSource = new();
    private Task? _eventHandlingTask;
    private IDisposable? _subscription;

    public QueuedObservable(ILogger<IHaContext> logger)
    {
        _logger = logger;
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        return _subject.Subscribe(observer);
    }

    public void Initialize(IObservable<T> innerObservable)
    {
        _subscription = innerObservable.Subscribe(e => _queue.Writer.TryWrite(e), onCompleted: () => _queue.Writer.Complete());
        _eventHandlingTask = Task.Run(async () => await HandleNewEvents().ConfigureAwait(false));
    }

    [SuppressMessage("", "CA1031")]
    private async Task HandleNewEvents()
    {
        await foreach(var @event in  _queue.Reader.ReadAllAsync(_tokenSource.Token).ConfigureAwait(false))
        {
            try
            {
                _subject.OnNext(@event);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception in subscription: ");
            }
        }
        _subject.OnCompleted();
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;
            
        // When disposed unsubscribe from inner observable
        _subscription?.Dispose();
            
        // mark the channel complete, this will break the processing loop when the last event in the channel is processed
        _queue.Writer.Complete();
        
        // now await for the processing loop to be ready so we know all pending events are processed
        if (_eventHandlingTask != null)
            await _eventHandlingTask.ConfigureAwait(false);
            
        _tokenSource.Dispose();        
        _subject.Dispose();
    }
}