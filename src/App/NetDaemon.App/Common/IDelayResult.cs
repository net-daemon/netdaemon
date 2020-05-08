using System;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    /// <summary>
    ///     Scheduler result lets you manage scheduled tasks like check completion, cancel the tasks etc.
    /// </summary>
    public interface IDelayResult : IDisposable
    {
        /// <summary>
        ///     Current running task to await, returns true if not canceled
        /// </summary>
        Task<bool> Task { get; }

        /// <summary>
        ///     Cancels the delay tasks and task will return false
        /// </summary>
        void Cancel();
    }
}