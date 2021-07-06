using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace NetDaemon.Common.Reactive
{
    #region IObservable<T> implementation

    /// <summary>
    ///     Implements the observable interface for state changes
    /// </summary>
    public class ObservableBase<T> : IObservable<T>
    {
        private readonly INetDaemonAppBase _app;

        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<IObserver<T>, IObserver<T>>
                            _observersTuples = new();
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="logger">A ILogger instance</param>
        /// <param name="app">App being tracked</param>
        public ObservableBase(ILogger logger, INetDaemonAppBase app)
        {
            _logger = logger;
            _app = app;
        }

        /// <summary>
        ///     List of current observers for a app
        /// </summary>
        public IEnumerable<IObserver<T>> Observers => _observersTuples.Values;
        /// <summary>
        ///     Clear all observers
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031")]

        public void Clear()
        {
            foreach (var eventObservable in _observersTuples)
            {
                try
                {
                    eventObservable.Value.OnCompleted();
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Error complete the observables for app {app}", _app.Id);
                }
            }
            _observersTuples.Clear();
        }

        /// <summary>
        ///     Subscribes to observable
        /// </summary>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (!_observersTuples.ContainsKey(observer))
                _observersTuples.TryAdd(observer, observer);

            return new UnsubscriberObservable<T>(_observersTuples, observer);
        }

        private class UnsubscriberObservable<X> : IDisposable
        {
            private readonly IObserver<X> _observer;
            private readonly ConcurrentDictionary<IObserver<X>, IObserver<X>> _observers;
            private bool disposedValue;

            public UnsubscriberObservable(
                ConcurrentDictionary<IObserver<X>, IObserver<X>> observers, IObserver<X> observer)
            {
                _observer = observer;
                _observers = observers;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        if (_observer is not null)
                        {
                            _observers.TryRemove(_observer, out _);
                            _observer.OnCompleted();
                        }
                    }

                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }

    #endregion IObservable<T> implementation
}