using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NetDaemon.HassModel.Common;

/// <summary>
///     Adds async and concurrent extensions on observables
/// </summary>
public static class ObservableExtensions
{
    /// <summary>
    ///     Allows calling async function but does this in a serial way. Long running tasks will block
    ///     other subscriptions
    /// </summary>
    public static IDisposable SubscribeAsync<T>(this IObservable<T> source, Func<T, Task> onNextAsync, Action<Exception>? onError = null)
    {
        return source
            .Select(e => Observable.FromAsync(() => HandleTask(onNextAsync, e, onError)))
            .Concat()
            .Subscribe();
    }

    // public static IDisposable Subscribe<T>([NotNull] this IObservable<T> source, [NotNull] Action<T> onNext)
    //     in class ObservableExtensions
    /// <summary>
    ///     Allows calling async function. Order of messages is not guaranteed
    /// </summary>
    public static IDisposable SubscribeAsyncConcurrent<T>(this IObservable<T> source, Func<T, Task> onNextAsync,
        Action<Exception>? onError = null)
    {
        return source
            .Select(async e => await Observable.FromAsync(() => HandleTask(onNextAsync, e, onError)))
            .Merge()
            .Subscribe();
    }    
    
    private static async Task HandleTask<T>(Func<T, Task> onNextAsync, T e, Action<Exception>? onError = null)
    {
        try
        {
            await onNextAsync(e).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (onError is not null)
            {
                onError(ex);
            }
            else
            {
                var logger = LoggerFactory.Create(x => { x.AddConsole(); }).CreateLogger<IHaContext>();

                logger.LogError(ex,
                    "SubscribeConcurrent throws an unhandled Exception, please use error callback function to do proper logging");
                throw;
            }
        }
    }

    /// <summary>
    ///     Allows calling async function with max nr of concurrent messages. Order of messages is not guaranteed
    /// </summary>
    public static IDisposable SubscribeAsyncConcurrent<T>(this IObservable<T> source, Func<T, Task> onNextAsync,
        int maxConcurrent)
    {
        return source
            .Select(e => Observable.FromAsync(() => HandleTask(onNextAsync, e)))
            .Merge(maxConcurrent)
            .Subscribe();
    }
}
