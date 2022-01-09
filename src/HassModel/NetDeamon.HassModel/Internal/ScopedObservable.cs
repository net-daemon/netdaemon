using System;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using NetDaemon.HassModel.Internal;
using Microsoft.Extensions.Logging;

namespace NetDaemon.Infrastructure.ObservableHelpers
{
    /// <summary>
    /// Wraps an Observable so all subscribers can be unsubscribed by disposing
    /// </summary>
    internal sealed class ScopedObservable<T> : IObservable<T>, IDisposable
    {
        private readonly IDisposable _subscription;
        private readonly Subject<T> _subject = new();
        private readonly ILogger _logger;

        public ScopedObservable(IObservable<T> innerObservable, ILogger logger)
        {
            _logger = logger;
            _subscription = innerObservable.Subscribe(_subject);
        }

        public IDisposable Subscribe(IObserver<T> observer) => _subject.Subscribe(observer);

        public void Dispose()
        {
            // When disposed unsubscribe from inner observable
            // this will make all subscribers of our Subject stop receiving events
            _subscription.Dispose();
            _subject.Dispose();
        }

        // private readonly ConcurrentDictionary<Task, object?> _backgroundTasks = new();
        // private void TrackObservableSubscriptionTasks(Task task, string? description = null)
        // {
        //     _backgroundTasks.TryAdd(task, null);
        //
        //     [SuppressMessage("", "CA1031")]
        //     async Task Wrap()
        //     {
        //         try
        //         {
        //             await task.ConfigureAwait(false);
        //         }
        //         catch (Exception e)
        //         {
        //             _logger.LogError(e, description == null ? null : "Exception in subscription: " + description);
        //         }
        //         finally
        //         {
        //             _backgroundTasks.TryRemove(task, out var _);
        //         }
        //     }
        //     // We do not handle task here cause exceptions
        //     // are handled in the Wrap local functions and
        //     // all tasks should be cancelable
        //     _ = Wrap();
        // }
    }
}

