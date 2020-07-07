using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetDaemon.Daemon;

namespace NetDaemon.Infrastructure.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        /// A version of Task.WhenAll that can be canceled.
        /// </summary>
        /// <param name="tasks">The tasks to wait for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static async Task WhenAll(this IEnumerable<Task> tasks, CancellationToken cancellationToken)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks), $"{nameof(tasks)} is null.");

            await Task.WhenAny(Task.WhenAll(tasks), cancellationToken.AsTask()).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}