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
        ///     Wait for state the specified
        /// </summary>
        /// <param name="observable"></param>
        /// <param name="timeout">Timeout waiting for state</param>
        public static IObservable<(EntityState Old, EntityState New)> NDWaitForState(this IObservable<(EntityState Old, EntityState New)> observable, TimeSpan timeout)
        {
            return observable.Timeout(timeout, Observable.Return((new NetDaemon.Common.EntityState() { State = "TimeOut" }, new NetDaemon.Common.EntityState() { State = "TimeOut" }))).Take(1);
        }

        /// <summary>
        ///     Wait for state the default time
        /// </summary>
        /// <param name="observable"></param>
        public static IObservable<(EntityState Old, EntityState New)> NDWaitForState(this IObservable<(EntityState Old, EntityState New)> observable)
        {
            return observable.Timeout(TimeSpan.FromSeconds(5), Observable.Return((new NetDaemon.Common.EntityState() { State = "TimeOut" }, new NetDaemon.Common.EntityState() { State = "TimeOut" }))).Take(1);
        }

    }
}