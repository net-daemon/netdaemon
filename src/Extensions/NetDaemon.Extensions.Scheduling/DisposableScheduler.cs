using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace NetDaemon.Extensions.Scheduler;

/// <summary>
/// IScheduler decorator that will cancel pending scheduled tasks when Disposed
/// </summary>
/// <remarks>
/// Wraps a scheduler to make it cancel all pending subscriptions on dispose.
/// </remarks>
public sealed class DisposableScheduler(IScheduler scheduler) : IScheduler, IDisposable
{
    private readonly IScheduler _innerScheduler = scheduler;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private int _isDisposed; // 0 = false, 1 = true

    /// <inheritdoc />
    public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
    {
        if (_isDisposed == 1) return Disposable.Empty;

        CancellationTokenRegistration? cancellationTokenRegistration = null;

        var subscription = _innerScheduler.Schedule(state, (_, d) =>
        {
            if (_isDisposed == 1) return Disposable.Empty;
            // pass this to the action to make sure it will also use this scheduler to schedule subsequent tasks
            var disposable = action(this, d);
            cancellationTokenRegistration?.Unregister();
            return disposable;
        });

        cancellationTokenRegistration = _cancellationTokenSource.Token.Register(() => subscription.Dispose());

        return subscription;
    }

    /// <inheritdoc />
    public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        if (_isDisposed == 1) return Disposable.Empty;

        CancellationTokenRegistration? cancellationTokenRegistration = null;

        var subscription = _innerScheduler.Schedule(state, dueTime, (_, d) =>
        {
            if (_isDisposed == 1) return Disposable.Empty;
            // pass this to the action to make sure it will also use this scheduler to schedule subsequent tasks
            var disposable = action(this, d);
            cancellationTokenRegistration?.Unregister();
            return disposable;
        });

        cancellationTokenRegistration = _cancellationTokenSource.Token.Register(() => subscription.Dispose());

        return subscription;
    }

    /// <inheritdoc />
    public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        if (_isDisposed == 1) return Disposable.Empty;

        CancellationTokenRegistration? cancellationTokenRegistration = null;

        var subscription = _innerScheduler.Schedule(state, dueTime, (_, d) =>
        {
            if (_isDisposed == 1) return Disposable.Empty;
            // pass this to the action to make sure it will also use this scheduler to schedule subsequent tasks
            var disposable = action(this, d);
            cancellationTokenRegistration?.Unregister();
            return disposable;
        });

        cancellationTokenRegistration = _cancellationTokenSource.Token.Register(() => subscription.Dispose());

        return subscription;
    }

    /// <inheritdoc />
    public DateTimeOffset Now => _innerScheduler.Now;

    /// <inheritdoc />
    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
            return;

        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}
