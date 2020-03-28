
using System.Collections.Generic;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    /// <summary>
    ///     Useful extension methods used
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///     Converts a valuepair to dynamic object
        /// </summary>
        /// <param name="attributeNameValuePair"></param>
        public static dynamic ToDynamic(this (string name, object val)[] attributeNameValuePair)
        {
            // Convert the tuple name/value pair to tuple that can be serialized dynamically
            var attributes = new FluentExpandoObject(true, true);
            foreach (var (attribute, value) in attributeNameValuePair)
            {
                ((IDictionary<string, object>)attributes).Add(attribute, value);
            }

            dynamic result = attributes;
            return result;
        }
    }
}