using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NetDaemon.Common.Exceptions;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.Daemon.Config
{
    public static class TaskExtensions
    {
        public static async Task InvokeAsync(this MethodInfo mi, object? obj, params object?[]? parameters)
        {
            _ = mi ??
                throw new NetDaemonArgumentNullException(nameof(mi));

            dynamic? awaitable = mi.Invoke(obj, parameters);
            if (awaitable != null)
                await awaitable.ConfigureAwait(false);
        }
    }

    public static class ConfigStringExtensions
    {
        public static string ToPascalCase(this string str)
        {
            _ = str ??
                throw new NetDaemonArgumentNullException(nameof(str));

            var build = new StringBuilder();
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
    }
}