using System;
using System.Collections.Generic;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{

    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class HomeAssistantServiceCallAttribute : System.Attribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        // This is a positional argument
        public HomeAssistantServiceCallAttribute()
        {
        }

     }

    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class HomeAssistantStateChangedAttribute : System.Attribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        string _entityId;
        private readonly object? _to;
        private readonly object? _from;
        private readonly bool _allChanges;

        // This is a positional argument
        public HomeAssistantStateChangedAttribute(string entityId, object? to=null, object? from=null, bool allChanges=false)
        {
            _entityId = entityId;
            _to = to;
            _from = from;
            _allChanges = allChanges;
        }

        public string EntityId => _entityId;

        public bool AllChanges => _allChanges;
        public object? From => _from;
        public object? To => _to;

    }


}