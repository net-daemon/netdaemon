using System.Reflection;

namespace NetDaemon.AppModel.Internal.AppFactories;

internal class ClassAppFactory(Type type, string? id = default, bool? focus = default) : IAppFactory
{
    public object Create(IServiceProvider provider)
    {
        return ActivatorUtilities.CreateInstance(provider, type);
    }

    public string Id { get; } = id ?? GetAppId(type);

    public bool HasFocus { get; } = focus ?? type.GetCustomAttribute<FocusAttribute>() is not null;

    private static string GetAppId(Type type)
    {
        var attribute = type.GetCustomAttribute<NetDaemonAppAttribute>();
        var id = attribute?.Id ?? type.FullName;

        if (string.IsNullOrEmpty(id))
        {
            throw new InvalidOperationException($"Could not get app id from {type}");
        }

        return id;
    }
}
