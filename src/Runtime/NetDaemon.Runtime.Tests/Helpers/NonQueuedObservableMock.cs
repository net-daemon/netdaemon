using System.Reactive.Subjects;
using NetDaemon.HassModel.Common;
using NetDaemon.Infrastructure.ObservableHelpers;

namespace NetDaemon.Runtime.Tests.Helpers;

/// <summary>
/// Non queued to allow easier testing
/// </summary>
internal sealed class NonQueuedObservableMock<T> : IQueuedObservable<T>
{
    private IDisposable? _subscription;
    private readonly Subject<T> _subject = new();

    public IDisposable Subscribe(IObserver<T> observer) => _subject.Subscribe(observer);

    public void Initialize(IObservable<T> innerObservable)
    {
        _subscription = innerObservable.Subscribe(_subject);
    }

    public ValueTask DisposeAsync()
    {
        // When disposed unsubscribe from inner observable
        // this will make all subscribers of our Subject stop receiving events
        _subscription?.Dispose();
        _subject.Dispose();
        return ValueTask.CompletedTask;
    }
}