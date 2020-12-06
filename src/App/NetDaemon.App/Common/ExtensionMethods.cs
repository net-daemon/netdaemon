using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Text;
using NetDaemon.Common.Fluent;

namespace NetDaemon.Common
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

        /// <summary>
        ///     Converts a anoumous type to expando object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ExpandoObject? ToExpandoObject(this object obj)
        {
            // Null-check

            IDictionary<string, object?> expando = new ExpandoObject();

            foreach (PropertyDescriptor? property in TypeDescriptor.GetProperties(obj.GetType()))
            {
                if (property is object)
                    expando.Add(property.Name, property.GetValue(obj));
            }

            return (ExpandoObject)expando;
        }

        /// <summary>
        ///     Converts any unicode string to a safe Home Assistant name
        /// </summary>
        /// <param name="str">The unicode string to convert</param>
        public static string ToSafeHomeAssistantEntityId(this string str)
        {
            string normalizedString = str.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder(str.Length);

            foreach (char c in normalizedString)
            {
                switch (CharUnicodeInfo.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                        stringBuilder.Append(c);
                        break;

                    case UnicodeCategory.SpaceSeparator:
                    case UnicodeCategory.ConnectorPunctuation:
                    case UnicodeCategory.DashPunctuation:
                        stringBuilder.Append('_');
                        break;
                }
            }
            return stringBuilder.ToString().ToLowerInvariant();
        }
    }
}