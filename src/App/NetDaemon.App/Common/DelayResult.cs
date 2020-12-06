using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.Common
{
    /// <summary>
    ///     A class that implements the management of delays and cancel them
    /// </summary>
    public class DelayResult : IDelayResult
    {
        private readonly INetDaemonApp _daemonApp;
        private readonly TaskCompletionSource<bool> _delayTaskCompletionSource;
        private bool _isCanceled = false;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="delayTaskCompletionSource"></param>
        /// <param name="daemonApp"></param>
        public DelayResult(TaskCompletionSource<bool> delayTaskCompletionSource, INetDaemonApp daemonApp)
        {
            _delayTaskCompletionSource = delayTaskCompletionSource;
            _daemonApp = daemonApp;
        }

        /// <inheritdoc/>
        public Task<bool> Task => _delayTaskCompletionSource.Task;

        internal ConcurrentBag<string> StateSubscriptions { get; set; } = new();

        /// <inheritdoc/>
        public void Cancel()
        {
            if (_isCanceled)
                return;

            _isCanceled = true;
            foreach (var stateSubscription in StateSubscriptions)
            {
                //Todo: Handle
                _daemonApp.CancelListenState(stateSubscription);
            }
            StateSubscriptions.Clear();

            // Also cancel all await if this is disposed
            _delayTaskCompletionSource.TrySetResult(false);
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        ///     Disposes the object and cancel delay
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        /// <summary>
        ///     Disposes the object and cancel delay
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Make sure any subscriptions are canceled
                    Cancel();
                }
                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}