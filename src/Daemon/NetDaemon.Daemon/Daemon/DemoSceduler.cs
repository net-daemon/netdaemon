using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Daemon
{
    /// <summary>
    /// Async friendly Timer implementation.
    /// Provides a mechanism for executing an async method on
    /// a thread pool thread at specified intervals.
    ///
    /// This class cannot be inherited.
    /// </summary>
    public sealed class TimerAsync : IDisposable
    {
        private readonly Func<CancellationToken, Task> _scheduledAction;
        private readonly TimeSpan _dueTime;
        private readonly TimeSpan _period;
        private CancellationTokenSource _cancellationSource;
        private Task _scheduledTask;
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;
        private readonly bool _canStartNextActionBeforePreviousIsCompleted;

        /// <summary>
        /// Occurs when an error is raised in the scheduled action
        /// </summary>
        public event EventHandler<Exception> OnError;

        /// <summary>
        /// Gets the running status of the TimerAsync instance. 
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Initializes a new instance of the TimerAsync. 
        /// </summary>
        /// <param name="scheduledAction">A delegate representing a method to be executed.</param>
        /// <param name="dueTime">The amount of time to delay befoe scheduledAction is invoked for the first time.</param>
        /// <param name="period">The time interval between invocations of the scheduledAction.</param>
        /// <param name="canStartNextActionBeforePreviousIsCompleted">
        ///   Whether or not the interval starts at the end of the previous scheduled action or at precise points in time. 
        /// </param>
        public TimerAsync(Func<CancellationToken, Task> scheduledAction, TimeSpan dueTime, TimeSpan period, bool canStartNextActionBeforePreviousIsCompleted = false)
        {
            _scheduledAction = scheduledAction ?? throw new ArgumentNullException(nameof(scheduledAction));

            if (dueTime < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(dueTime), "due time must be equal or greater than zero");
            _dueTime = dueTime;

            if (period < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(period), "period must be equal or greater than zero");
            _period = period;

            _canStartNextActionBeforePreviousIsCompleted = canStartNextActionBeforePreviousIsCompleted;

            _semaphore = new SemaphoreSlim(1);
        }

        /// <summary>
        /// Starts the TimerAsync.
        /// </summary>
        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            _semaphore.Wait();

            try
            {
                if (IsRunning)
                    return;

                _cancellationSource = new CancellationTokenSource();
                _scheduledTask = RunScheduledAction();
                IsRunning = true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Stops the TimerAsync.
        /// </summary>
        /// <returns>A task that completes when the timer is stopped.</returns>
        public async Task Stop()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (!IsRunning)
                    return;

                _cancellationSource.Cancel();

                await _scheduledTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
            finally
            {
                IsRunning = false;
                _semaphore.Release();
            }
        }

        private Task RunScheduledAction()
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(_dueTime, _cancellationSource.Token).ConfigureAwait(false);

                    while (true)
                    {
                        if (_canStartNextActionBeforePreviousIsCompleted)
#pragma warning disable 4014
                            _scheduledAction(_cancellationSource.Token);
#pragma warning restore 4014
                        else
                            await _scheduledAction(_cancellationSource.Token).ConfigureAwait(false);

                        await Task.Delay(_period, _cancellationSource.Token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    try
                    {
                        OnError?.Invoke(this, ex);
                    }
                    catch
                    {
                        // ignored
                    }
                }
                finally
                {
                    IsRunning = false;
                }
            }, _cancellationSource.Token);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            // NOTE: release unmanaged resources here

            if (disposing)
            {
                _cancellationSource?.Dispose();
                _semaphore?.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        /// Releases all resources used by the current instance of TimerAsync.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TimerAsync()
        {
            Dispose(false);
        }
    }
}
