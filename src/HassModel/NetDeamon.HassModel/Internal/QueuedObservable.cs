using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Common;

namespace NetDaemon.Infrastructure.ObservableHelpers;

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
        _subscription = innerObservable.Subscribe(e => _queue.Writer.TryWrite(e));
        _eventHandlingTask = Task.Run(async () => await HandleNewEvents().ConfigureAwait(false));
    }

    [SuppressMessage("", "CA1031")]
    private async Task HandleNewEvents()
    {
        while (!_tokenSource.IsCancellationRequested)
        {
            var @event = await _queue.Reader.ReadAsync(_tokenSource.Token).ConfigureAwait(false);
            try
            {
                _subject.OnNext(@event);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception in subscription: ");
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        // When disposed unsubscribe from inner observable
        // this will make all subscribers of our Subject stop receiving events
        _subscription?.Dispose();
        _tokenSource.Cancel();

        if (_eventHandlingTask != null)
        {
            try
            {
                await _eventHandlingTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Ignore, it should happen
            }
        }
        _tokenSource.Dispose();        
        _subject.Dispose();
    }
}