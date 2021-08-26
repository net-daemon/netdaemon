using System;
using NetDaemon.Common;

namespace Model3.ModelV3
{
    public interface IEventProvider
    {
        // TODO: not sure yet if this should be part of IHaCOntext or as a separate interface
        
        IObservable<T> GetEventDataOfType<T>(string eventType) where T : class;
    }
}