using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    public interface IFluentEvent
    {
        IExecute Call(Func<string, dynamic?, Task> func);
    }

    public class FluentEventManager : IFluentEvent, IExecute
    {
        private IEnumerable<string> _events;
        private INetDaemon _daemon;
        private Func<string, dynamic, Task> _functionToCall;
        private Func<FluentEventProperty, bool> _funcSelector;
      
        public FluentEventManager(IEnumerable<string> events, INetDaemon daemon)
        {
            _events = events;
            _daemon = daemon;
        }

        public FluentEventManager(Func<FluentEventProperty, bool> func, INetDaemon daemon)
        {
            _funcSelector = func;
            _daemon = daemon;
        }

        public IExecute Call(Func<string, dynamic, Task> func)
        {
            if (func == null)
                throw new NullReferenceException("Call function is null listening to event");

            _functionToCall = func;
            return this;
        }
        public void Execute()
        {
            if (_events != null)
                foreach (var ev in _events)
                    _daemon.ListenEvent(ev, _functionToCall!);

            if (_funcSelector != null)
            {
                _daemon.ListenEvent(_funcSelector, _functionToCall!);
            }
        }
    }

    public class FluentEventProperty
    {
        public string EventId { get; set; } = "";
        public dynamic? Data { get; set; } = null;
    }
}
