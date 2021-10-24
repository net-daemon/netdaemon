using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NetDaemon.HassModel.CodeGenerator.Extensions
{
    internal static class StringExtensions
    {
        public static string ToNormalizedPascalCase(this string name, string prefix = "HA_")
        {
            return name.ToPascalCase().ToNormalized(prefix);
        }

        public static string ToNormalizedCamelCase(this string name, string prefix = "HA_")
        {
            return name.ToCamelCase().ToNormalized(prefix);
        }

        private static string ToNormalized(this string name, string prefix = "HA_")
        {
            name = name.Replace(".", "_", StringComparison.InvariantCulture);

            if (!char.IsLetter(name[0]) && name[0] != '_')
                name = prefix + name;

            return Regex.Replace(name, "[^a-zA-Z0-9]+", "", RegexOptions.Compiled);
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
}