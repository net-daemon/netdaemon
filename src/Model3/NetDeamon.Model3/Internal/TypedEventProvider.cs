using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using JoySoftware.HomeAssistant.Model;
using NetDaemon.Model3.Common;

namespace NetDaemon.Model3.Internal
{

    [SuppressMessage("", "CA1812", Justification = "Is Loaded via DependencyInjection")]
    internal class TypedEventProvider : IEventProvider
    {
        private readonly IObservable<HassEvent> _hassEventsObservable;

        public TypedEventProvider(IObservable<HassEvent> hassEventsObservable)
        {
            _hassEventsObservable = hassEventsObservable;
        }

        public IObservable<T> GetEventDataOfType<T>(string eventType) where T : class =>
            _hassEventsObservable
                .Where(e => e.EventType == eventType && e.DataElement != null)
                .Select(e => e.DataElement?.ToObject<T>()!);
    }
}