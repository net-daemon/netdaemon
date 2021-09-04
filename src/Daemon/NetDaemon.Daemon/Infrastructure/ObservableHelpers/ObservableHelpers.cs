using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NetDaemon.Infrastructure.ObservableHelpers
{
    static class ObservableHelpers
    {
        /// <summary>
        /// IObservable wrapper that calls each subscriber in a separate background task
        /// </summary>
        public static IObservable<T> AsConcurrent<T>(this IObservable<T> observable, Action<Task> trackTask) =>
            Observable.Create<T>(subscribe: a => observable.Subscribe(a.AsConcurrent(trackTask)));

        /// <summary>
        /// IObserver wrapper that calls OnNext in a separate background task that can be tracked
        /// </summary>
        private static IObserver<T> AsConcurrent<T>(this IObserver<T> inner, Action<Task> trackTask)
            => Observer.Create<T>(onNext: obj =>
            {
                var task = Task.Run(() => inner.OnNext(obj));
                trackTask(task);
            });
    }
}

