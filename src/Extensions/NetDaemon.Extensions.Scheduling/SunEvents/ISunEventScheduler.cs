using System;
using System.Collections.Generic;
using System.Text;

namespace NetDaemon.Extensions.Scheduler.SunEvents
{
    /// <summary>
    ///     Provides scheduling capability based on sun events
    /// </summary>
    public interface ISunEventScheduler
    {
        /// <summary>
        ///     Runs at action at Sunset based on configured coordinates
        /// </summary>
        /// <param name="action">Action to run</param>
        IDisposable RunAtSunset(Action action);

        /// <summary>
        ///     Runs at action at Dawn (Civil) based on configured coordinates
        /// </summary>
        /// <param name="action">Action to run</param>
        IDisposable RunAtDawn(Action action);

        /// <summary>
        ///     Runs at action at Sunrise based on configured coordinates
        /// </summary>
        /// <param name="action">Action to run</param>
        IDisposable RunAtSunrise(Action action);

        /// <summary>
        ///     Runs at action at Dusk (Civil) based on configured coordinates
        /// </summary>
        /// <param name="action">Action to run</param>
        IDisposable RunAtDusk(Action action);
    }
}
