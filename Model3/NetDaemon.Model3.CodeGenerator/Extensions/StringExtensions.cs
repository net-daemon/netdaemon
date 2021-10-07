using System;
using System.Text.RegularExpressions;
using NetDaemon.Daemon.Config;

namespace NetDaemon.Model3.CodeGenerator.Extensions
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
    }
}