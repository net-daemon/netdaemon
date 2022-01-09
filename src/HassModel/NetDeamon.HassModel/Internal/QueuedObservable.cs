using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Common;

namespace NetDaemon.Infrastructure.ObservableHelpers
{
    /// <summary>
    /// Wraps an Observable so all subscribers can be unsubscribed by disposing
    /// </summary>
    internal sealed class QueuedObservable<T> : IQueuedObservable<T>
    {
        private IDisposable? _subscription;
        private readonly Subject<T> _subject = new();
        private readonly ILogger<IHaContext> _logger;

        private readonly Channel<T> _queue = Channel.CreateBounded<T>(100);
        private readonly CancellationTokenSource _tokenSource = new();
        private Task?_eventHandlingTask;
        
        public QueuedObservable(ILogger<IHaContext> logger)
        { 
            _logger = logger;
        }

        private async Task HandleNewEvents()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
                var @event = await _queue.Reader.ReadAsync(_tokenSource.Token).ConfigureAwait(false);
                try
                {
                    _subject.OnNext(@event);
                }
                catch (OperationCanceledException)
                {
                    // ignore and exit
                    break;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception in subscription: ");
                }
            }
        }
        
        public IDisposable Subscribe(IObserver<T> observer) => _subject.Subscribe(observer);

        public void Initialize(IObservable<T> innerObservable)
        {
            _subscription = innerObservable.Subscribe(e => _queue.Writer.TryWrite(e));
            _eventHandlingTask = Task.Run(async () => await HandleNewEvents().ConfigureAwait(false));
        }

        public void Dispose()
        {
            // When disposed unsubscribe from inner observable
            // this will make all subscribers of our Subject stop receiving events
            _subscription?.Dispose();
            _subject.Dispose();
            if (!_tokenSource.IsCancellationRequested)
                _tokenSource.Cancel();
            _tokenSource.Dispose();
        }
    }
}

