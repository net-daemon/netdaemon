using System.Reactive.Concurrency;

namespace NetDaemon.HassModel.Internal;

/// <summary>
///     Useful extension methods used
/// </summary>
internal static class NetDaemonExtensions
{
    public static (string? Left, string Right) SplitAtDot(this string id)
    {
        var firstDot = id.IndexOf('.', System.StringComparison.InvariantCulture);
        return firstDot == -1 ? ((string? Left, string Right))(null, id)
            : ((string? Left, string Right))(id[.. firstDot ], id[ (firstDot + 1) .. ]);

    }

    /// <summary>
    /// Special throttle mechanism used for RegistryCache updates. This will directly forward the first event,
    /// but then delays subsequent events to make sure there is a minimum time between refreshing the cache.
    /// If a new event is received while a previous one was waiting, the previous is dropped
    /// </summary>
    public static IObservable<T> ThrottleAfterFirstEvent<T>(this IObservable<T> observable, TimeSpan minDelay, IScheduler scheduler)
    {
        DateTimeOffset holdOffUntil = DateTimeOffset.MinValue;
        IDisposable? lastSchedule = null;

        return Observable.Create<T>(observer => observable.Subscribe(
            onNext: e =>
            {
                // If we previously postponed forwarding an event, it will be replaced by this newer event
                lastSchedule?.Dispose();

                if (holdOffUntil < scheduler.Now)
                {
                    observer.OnNext(e);
                    holdOffUntil = scheduler.Now + minDelay;
                }
                else
                {
                    // not sending this event yet, schedule in the future
                    lastSchedule = scheduler.Schedule(holdOffUntil, () =>
                    {
                        observer.OnNext(e);
                        holdOffUntil = scheduler.Now + minDelay;
                    });
                }
            },
            onError: observer.OnError,
            onCompleted: observer.OnCompleted)
        );
    }
}
