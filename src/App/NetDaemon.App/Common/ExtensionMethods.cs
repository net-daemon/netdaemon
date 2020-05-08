using System.Collections.Generic;
using System.Globalization;
using System.Text;

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