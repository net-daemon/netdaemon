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
    public static string ToEntityIdFromApplicationId(string applicationId, bool isDevelopment = false) =>
        !isDevelopment ?
            $"input_boolean.netdaemon_{ToSafeVersion(applicationId)}" :
            $"input_boolean.dev_netdaemon_{ToSafeVersion(applicationId)}" ;

    [SuppressMessage("Microsoft.Globalization", "CA1308")]
    [SuppressMessage("", "CA1062")]
    private static string ToSafeVersion(string applicationId)
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

        return stringBuilder.ToString().ToLowerInvariant();
    }
}
