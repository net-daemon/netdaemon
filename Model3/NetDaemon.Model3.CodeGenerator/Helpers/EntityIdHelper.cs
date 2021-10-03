using System;
namespace NetDaemon.Service.App.CodeGeneration.Helpers
{
    internal static class EntityIdHelper
    {
        public static string GetDomain(string str)
        {
            return str[..str.IndexOf(".", StringComparison.InvariantCultureIgnoreCase)];
        }

        public static string GetEntity(string str)
        {
            return str[(str.IndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1)..];
        }
    }
}