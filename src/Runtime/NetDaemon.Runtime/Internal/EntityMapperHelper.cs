using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace NetDaemon.Runtime.Internal;

public static class EntityMapperHelper
{
    /// <summary>
    ///     Converts any unicode string to a safe Home Assistant name for the helper
    /// </summary>
    /// <param name="applicationId">The unicode string to convert</param>
    [SuppressMessage("Microsoft.Globalization", "CA1308")]
    [SuppressMessage("", "CA1062")]
    public static string ToSafeHomeAssistantEntityIdFromApplicationId(string applicationId)
    {
        var normalizedString = applicationId.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new(applicationId.Length);

        var lastChar = '\0';

        foreach (var c in normalizedString)
        {
            switch (CharUnicodeInfo.GetUnicodeCategory(c))
            {
                case UnicodeCategory.LowercaseLetter:
                    stringBuilder.Append(c);
                    break;
                case UnicodeCategory.UppercaseLetter:
                    if (CharUnicodeInfo.GetUnicodeCategory(lastChar) == UnicodeCategory.LowercaseLetter)
                        if (lastChar != '_')
                            stringBuilder.Append('_');
                    stringBuilder.Append(char.ToLowerInvariant(c));
                    break;
                case UnicodeCategory.DecimalDigitNumber:
                    stringBuilder.Append(c);
                    break;
                case UnicodeCategory.SpaceSeparator:
                case UnicodeCategory.ConnectorPunctuation:
                case UnicodeCategory.DashPunctuation:
                case UnicodeCategory.OtherPunctuation:
                    if (lastChar != '_')
                        stringBuilder.Append('_');
                    break;
            }

            lastChar = c;
        }

        return $"input_boolean.netdaemon_{stringBuilder.ToString().ToLowerInvariant()}";
    }
}
