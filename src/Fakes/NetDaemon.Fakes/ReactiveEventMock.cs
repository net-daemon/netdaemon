using System;
using NetDaemon.Common.Reactive;

namespace NetDaemon.Daemon.Fakes
{
    /// <summary>
    ///     Implements Observable RxEvent
    /// </summary>
    public class ReactiveEventMock<T> : IRxEvent where T: class, INetDaemonRxApp
    {
        private readonly RxAppMock<T> _daemonRxApp;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="daemon">The NetDaemon host object</param>
        public ReactiveEventMock(RxAppMock<T> daemon)
        {
            _daemonRxApp = daemon;
        }

        /// <summary>
        ///     Implements IObservable ReactiveEvent
        /// </summary>
        /// <param name="observer">Observer</param>
        public IDisposable Subscribe(IObserver<RxEvent> observer)
        {
            return _daemonRxApp!.EventChangesObservable.Subscribe(observer);
        }
    }
}