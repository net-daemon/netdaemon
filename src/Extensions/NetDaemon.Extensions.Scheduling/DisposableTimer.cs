using System.Reactive.Disposables;

namespace NetDaemon.Extensions.Scheduler;

/// <summary>
///     Implements a IDisposable to cancel timers
/// </summary>
public sealed class DisposableTimer : IDisposable
{
    private readonly CancellationTokenSource _combinedToken;
    private readonly CancellationTokenSource _internalToken;
    private bool _disposed;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="token">App cancellation token to combine</param>
    public DisposableTimer(CancellationToken token)
    {
        _internalToken = new CancellationTokenSource();
        _combinedToken = CancellationTokenSource.CreateLinkedTokenSource(_internalToken.Token, token);
    }

    /// <summary>
    ///    Empty disposable timer
    /// </summary>
    public static IDisposable Empty => Disposable.Empty;

    /// <summary>
    ///     Token to use as cancellation
    /// </summary>
    public CancellationToken Token => _combinedToken.Token;
    /// <summary>
    ///     Disposes and cancel timers
    /// </summary>
    public void Dispose()
    {
        if (!_disposed){
            _internalToken.Cancel();
            _combinedToken.Dispose();
            _internalToken.Dispose();
            _disposed = true;
        }
    }
}
