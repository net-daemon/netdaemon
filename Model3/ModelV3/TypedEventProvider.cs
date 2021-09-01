using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using JoySoftware.HomeAssistant.Model;

namespace Model3.ModelV3
{

    // TODO: Supressing the warning but it complains that the internal class never is instansiated
    [SuppressMessage("", "CA1812")]
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