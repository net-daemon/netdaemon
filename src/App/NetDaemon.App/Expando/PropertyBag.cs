using System;
using System.Collections.Generic;

using System.Xml.Serialization;
using System.Xml;

namespace JoySoftware.HomeAssistant.NetDaemon.Expando
{
    /// <summary>
    /// Creates an XML serializable string/object dictionary.
    /// Encodes keys as element names and values as simple values with a type
    /// attribute that contains an XML type name. Complex names encode the type 
    /// name with type='___namespace.classname' format followed by a standard xml
    /// serialized format. The latter serialization can be slow so it's not recommended
    /// to pass complex types if performance is critical.    
    /// </summary>
    public class PropertyBag : PropertyBag<object>
    {
       
    }

    /// <summary>
    /// Creates a serializable string for generic types that is XML serializable.
    /// 
    /// Encodes keys as element names and values as simple values with a type
    /// attribute that contains an XML type name. Complex names encode the type 
    /// name with type='___namespace.classname' format followed by a standard xml
    /// serialized format. The latter serialization can be slow so it's not recommended
    /// to pass complex types if performance is critical.
    /// </summary>
    /// <typeparam name="TValue">Must be a reference type. For value types use type object</typeparam>
    public class PropertyBag<TValue> : Dictionary<string, TValue>
    {
    }
}
