using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NetDaemon.HassModel.CodeGenerator.Extensions;

internal static class StringExtensions
{
    public static string ToValidCSharpPascalCase(this string name)
    {
        return name.ToPascalCase().ToValidCSharpIdentifier();
    }

    public static string ToValidCSharpCamelCase(this string name)
    {
        return name.ToCamelCase().ToValidCSharpIdentifier();
    }

    public static string ToValidCSharpIdentifier(this string name)
    {
        name = name.Replace(".", "_", StringComparison.InvariantCulture);

        name = Regex.Replace(name, "[^a-zA-Z0-9_]+", "", RegexOptions.Compiled);

        if (name.Length == 0 || char.IsAsciiDigit(name[0]))
            name = "_" + name;

        return name;
    }

    public static string ToPascalCase(this string str)
    {
        var build = new StringBuilder(str.Length);
        bool nextIsUpper = false;
        bool isFirstCharacter = true;
        foreach (char c in str)
        {
            if (c == '_')
            {
                nextIsUpper = true;
                continue;
            }

            build.Append(nextIsUpper || isFirstCharacter ? char.ToUpper(c, CultureInfo.InvariantCulture) : c);
            nextIsUpper = false;
            isFirstCharacter = false;
        }

        return build.ToString();
    }

    public static string ToCamelCase(this string str)
    {
        var camelCaseStr = ToPascalCase(str);

        return char.ToLowerInvariant(camelCaseStr[0]) + camelCaseStr[1..];
    }
}
