using System;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading.Tasks;
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

        public ScopedObservable(IObservable<T> innerObservable)
        {
            // Make a single subscription on the innerObservable 
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
    }
}

