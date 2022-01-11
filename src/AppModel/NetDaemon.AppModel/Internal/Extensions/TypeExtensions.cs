using System;
using System.Reflection;
namespace NetDaemon.AppModel.Internal.Extensions;

internal static class TypeExtensions
{
    public static bool HasNetDaemonFocusAttribute(this Type type)
    {
        return Attribute.GetCustomAttribute(type, typeof(FocusAttribute)) is not null;
    }

    public static bool IsNetDaemonFocusAttributeUsed(this IEnumerable<Type> types)
    {
        return types.Any(n => n.IsClass &&
                         !n.IsGenericType &&
                         !n.IsAbstract &&
                         n.GetCustomAttribute<FocusAttribute>() is not null);
    }
}