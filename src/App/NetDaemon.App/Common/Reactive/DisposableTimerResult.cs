using System;
using System.Threading;

namespace NetDaemon.Common.Reactive
{
    /// <summary>
    ///     Implements a IDisposable to cancel timers
    /// </summary>
    public sealed class DisposableTimerResult : IDisposable
    {
        private readonly CancellationTokenSource _combinedToken;
        private readonly CancellationTokenSource _internalToken;
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="token">App cancellation token to combine</param>
        public DisposableTimerResult(CancellationToken token)
        {
            _internalToken = new CancellationTokenSource();
            _combinedToken = CancellationTokenSource.CreateLinkedTokenSource(_internalToken.Token, token);
        }

        /// <summary>
        ///     Token to use as cancellation
        /// </summary>
        public CancellationToken Token => _combinedToken.Token;
        /// <summary>
        ///     Disposes and cancel timerrs
        /// </summary>
        public void Dispose()
        {
            _internalToken.Cancel();
        }
    }
}