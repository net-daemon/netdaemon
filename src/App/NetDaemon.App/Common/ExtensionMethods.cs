using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace NetDaemon.Common
{
    /// <summary>
    ///     Useful extension methods used
    /// </summary>
    public static class NetDaemonExtensions
    {
        /// <summary>
        ///     Converts valuepairs to dynamic object
        /// </summary>
        /// <param name="attributeNameValuePairs"></param>
        [SuppressMessage("", "CA1062")]
        public static dynamic ToDynamic(this (string name, object val)[] attributeNameValuePairs)
        {
            // Convert the tuple name/value pair to tuple that can be serialized dynamically
            var attributes = new FluentExpandoObject(true, true);
            foreach (var (attribute, value) in attributeNameValuePairs)
            {
                // We only add non-null values since the FluentExpandoObject will 
                // return null on missing anyway
                attributes.Add(attribute, value);
            }

            return attributes;
        }

        /// <summary>
        ///     Converts a valuepair to dynamic object
        /// </summary>
        /// <param name="attributeNameValuePair"></param>
        public static dynamic ToDynamic(this (string name, object val) attributeNameValuePair)
        {
            return ToDynamic(new[] { attributeNameValuePair });
        }

        /// <summary>
        ///     Converts a anoumous type to expando object
        /// </summary>
        /// <param name="obj"></param>
        [SuppressMessage("", "CA1062")]
        public static ExpandoObject ToExpandoObject(this object obj)
        {
            IDictionary<string, object?> expando = new ExpandoObject();

            foreach (PropertyDescriptor? property in TypeDescriptor.GetProperties(obj.GetType()))
            {
                if (property is not null)
                    expando.Add(property.Name, property.GetValue(obj));
            }

            return (ExpandoObject)expando;
        }

        internal static object? ToDynamicValue(this JsonElement elem)
        {
            return elem.ValueKind switch
            {
                JsonValueKind.String => ParseDataType(elem.GetString()),
                JsonValueKind.False => false,
                JsonValueKind.True => true,
                JsonValueKind.Number => elem.TryGetInt64(out long intValue) ? intValue : elem.GetDouble(),
                _ => null,
            };
        }

        private static object? ParseDataType(string? state)
        {
            if (long.TryParse(state, NumberStyles.Number, CultureInfo.InvariantCulture, out long intValue))
                return intValue;

            if (double.TryParse(state, NumberStyles.Number, CultureInfo.InvariantCulture, out double doubleValue))
                return doubleValue;

            if (state == "unavailable")
                return null;

            return state;
        }

        /// <summary>
        ///     Converts any unicode string to a safe Home Assistant name
        /// </summary>
        /// <param name="str">The unicode string to convert</param>
        [SuppressMessage(category: "Microsoft.Globalization", checkId: "CA1308")]
        [SuppressMessage("", "CA1062")]
        public static string ToSafeHomeAssistantEntityId(this string str)
        {
            string normalizedString = str.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new(str.Length);

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