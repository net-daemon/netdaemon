using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.Config
{
    // public interface IDaemonAppConfig
    // {
    //     Task InstanceFromDaemonAppConfigs(IEnumerable<Type> netDaemonApps, string codeFolder);
    // }
    public static class TaskExtensions
    {
        public static async Task InvokeAsync(this MethodInfo mi, object? obj, params object?[]? parameters)
        {
            dynamic? awaitable = mi.Invoke(obj, parameters);
            await awaitable;
        }
    }

    public static class ConfigStringExtensions
    {
        public static string ToPythonStyle(this string str)
        {
            var build = new StringBuilder(str.Length);
            bool isStart = true;
            foreach (char c in str)
            {
                if (char.IsUpper(c) && !isStart)
                    build.Append("_");
                else
                    isStart = false;
                build.Append(char.ToLower(c));
            }
            return build.ToString();
        }

        public static string ToCamelCase(this string str)
        {
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

                build.Append(nextIsUpper || isFirstCharacter ? char.ToUpper(c) : c);
                nextIsUpper = false;
                isFirstCharacter = false;
            }
            var returnString = build.ToString();

            return build.ToString();
        }
    }
}