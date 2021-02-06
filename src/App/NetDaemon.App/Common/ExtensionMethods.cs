using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Text;
using System.Diagnostics.CodeAnalysis;
namespace NetDaemon.Common
{
    /// <summary>
    ///     Useful extension methods used
    /// </summary>
    public static class NetDaemonExtensions
    {
        /// <summary>
        ///     Converts a valuepair to dynamic object
        /// </summary>
        /// <param name="attributeNameValuePair"></param>
        public static dynamic ToDynamic(this (string name, object val)[] attributeNameValuePair)
        {
            // Convert the tuple name/value pair to tuple that can be serialized dynamically
            var attributes = new FluentExpandoObject(true, true);
            if (attributeNameValuePair is not null)
            {
                foreach (var (attribute, value) in attributeNameValuePair)
                {
                    if (value is not null)
                    {
                        // We only add non-null values since the FluentExpandoObject will 
                        // return null on missing anyway
                        attributes.Add(attribute, value);
                    }
                }
            }

            return (dynamic)attributes;
        }

        /// <summary>
        ///     Converts a anoumous type to expando object
        /// </summary>
        /// <param name="obj"></param>
        public static ExpandoObject? ToExpandoObject(this object obj)
        {
            // Null-check

            IDictionary<string, object?> expando = new ExpandoObject();

            if (obj is not null)
            {
                foreach (PropertyDescriptor? property in TypeDescriptor.GetProperties(obj.GetType()))
                {
                    if (property is not null)
                        expando.Add(property.Name, property.GetValue(obj));
                }
            }

            return (ExpandoObject)expando;
        }

        /// <summary>
        ///     Converts any unicode string to a safe Home Assistant name
        /// </summary>
        /// <param name="str">The unicode string to convert</param>
        [SuppressMessage(category: "Microsoft.Globalization", checkId: "CA1308")]
        public static string ToSafeHomeAssistantEntityId(this string str)
        {
            if (str is null)
                return string.Empty;

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