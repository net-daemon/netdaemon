using System;
using System.Reactive.Linq;

namespace NetDaemon.Common.Reactive
{
    /// <summary>
    ///     Extension methods for Observables
    /// </summary>
    public static class ObservableExtensionMethods
    {
        /// <summary>
        ///     Is same for timespan time
        /// </summary>
        /// <param name="observable"></param>
        /// <param name="span"></param>
        /// <returns></returns>
        public static IObservable<(EntityState Old, EntityState New)> NDSameStateFor(this IObservable<(EntityState Old, EntityState New)> observable, TimeSpan span)
        {
            return observable.Throttle(span);
        }

        /// <summary>
        ///     Is same for timespan time
        /// </summary>
        /// <param name="observable"></param>
        /// <param name="timeout">Timeout waiting for state</param>
        /// <returns></returns>
        public static IObservable<(EntityState Old, EntityState New)> NDFirstOrTimeout(this IObservable<(EntityState Old, EntityState New)> observable, TimeSpan timeout)
        {
            return observable.Timeout(timeout, Observable.Return((new NetDaemon.Common.EntityState() { State = "TimeOut" }, new NetDaemon.Common.EntityState() { State = "TimeOut" }))).Take(1);
        }
    }
}