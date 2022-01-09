using System;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Common;
using NetDaemon.Infrastructure.ObservableHelpers;

namespace NetDaemon.HassModel.Tests.TestHelpers;

    /// <summary>
    /// Non queued to allow easier testing
    /// </summary>
    internal sealed class NonQueuedObservableMock<T> : IQueuedObservable<T>
    {
        private IDisposable? _subscription;
        private readonly Subject<T> _subject = new();
        private readonly ILogger<IHaContext> _logger;

        public NonQueuedObservableMock(ILogger<IHaContext> logger)
        { 
            _logger = logger;
        }

        public IDisposable Subscribe(IObserver<T> observer) => _subject.Subscribe(observer);

        public void Initialize(IObservable<T> innerObservable)
        {
            _subscription = innerObservable.Subscribe(_subject);
        }

        public void Dispose()
        {
            // When disposed unsubscribe from inner observable
            // this will make all subscribers of our Subject stop receiving events
            _subscription?.Dispose();
            _subject.Dispose();
        }
    }