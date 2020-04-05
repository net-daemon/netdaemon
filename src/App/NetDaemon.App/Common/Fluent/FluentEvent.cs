using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    /// <summary>
    ///     Handles events in fluent API
    /// </summary>
    public interface IFluentEvent
    {
        /// <summary>
        ///     The fuction to execute
        /// </summary>
        /// <param name="func"></param>
        IExecute Call(Func<string, dynamic?, Task>? func);
    }

    /// <summary>
    ///     Implements fluent events
    /// </summary>
    public class FluentEventManager : IFluentEvent, IExecute
    {
        private IEnumerable<string>? _events;
        private INetDaemon _daemon;
        private Func<string, dynamic, Task>? _functionToCall;
        private Func<FluentEventProperty, bool>? _funcSelector;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="events">The events to handle</param>
        /// <param name="daemon">NetDaemon that manage API calls to Home Assistant</param>
        public FluentEventManager(IEnumerable<string> events, INetDaemon daemon)
        {
            _events = events;
            _daemon = daemon;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="func">The lambda to filter events</param>
        /// <param name="daemon">NetDaemon that manage API calls to Home Assistant</param>
        public FluentEventManager(Func<FluentEventProperty, bool> func, INetDaemon daemon)
        {
            _funcSelector = func;
            _daemon = daemon;
        }

        /// <inheritdoc/>
        public IExecute Call(Func<string, dynamic, Task>? func)
        {
            if (func == null)
                throw new NullReferenceException("Call function is null listening to event");

            _functionToCall = func;
            return this;
        }

        /// <inheritdoc/>
        public void Execute()
        {
            if (_events == null && _funcSelector == null)
                throw new NullReferenceException($"Both {nameof(_events)} or {nameof(_events)} cant be null");

            if (_events != null)
                foreach (var ev in _events)
                    _daemon.ListenEvent(ev, _functionToCall!);
            else
                _daemon.ListenEvent(_funcSelector!, _functionToCall!);
        }
    }

    /// <summary>
    ///     Selector class for lambda expression selecting events
    /// </summary>
    public class FluentEventProperty
    {
        /// <summary>
        ///     Unique id of event
        /// </summary>
        public string EventId { get; set; } = "";

        /// <summary>
        ///
        /// </summary>
        /// <value></value>
        public dynamic? Data { get; set; } = null;
    }
}