using System.Reflection;

namespace NetDaemon.AppModel.Internal.AppFactories;

internal static class AppFactoryHelper
{
    public static string GetAppId(Type type)
    {
        var attribute = type.GetCustomAttribute<NetDaemonAppAttribute>();
        var id = attribute?.Id ?? type.FullName;

        if (string.IsNullOrEmpty(id))
        {
            throw new InvalidOperationException($"Could not get app id from {type}");
        }

        return id;
    }

    public static bool GetAppFocus(Type type)
    {
        return type.GetCustomAttribute<FocusAttribute>() is not null;
    }
}