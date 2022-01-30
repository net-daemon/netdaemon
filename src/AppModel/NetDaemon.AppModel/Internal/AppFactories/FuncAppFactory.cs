using System.Reflection;

namespace NetDaemon.AppModel.Internal.AppFactories;

internal class FuncAppFactory : IAppFactory
{
    private readonly Func<IServiceProvider, object> _func;

    private FuncAppFactory(Func<IServiceProvider, object> func, Type type, string? id, bool? focus)
    {
        _func = func;

        Id = id ?? GetAppId(type);
        HasFocus = focus ?? GetAppFocus(type);
    }
    
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

    private static bool GetAppFocus(Type type)
    {
        return type.GetCustomAttribute<FocusAttribute>() is not null;
    }

    public object Create(IServiceProvider provider)
    {
        return _func.Invoke(provider);
    }

    public string Id { get; }

    public bool HasFocus { get; }

    public static FuncAppFactory Create<TAppType>(Func<IServiceProvider, TAppType> func,
        string? id = default, bool? focus = default) where TAppType : class
    {
        return new FuncAppFactory(func, typeof(TAppType), id, focus);
    }

    public static FuncAppFactory Create(Type type,
        string? id = default, bool? focus = default)
    {
        return new FuncAppFactory(CreateFactoryFunc(type), type, id, focus);
    }

    private static Func<IServiceProvider, object> CreateFactoryFunc(Type type)
    {
        return provider => ActivatorUtilities.CreateInstance(provider, type);
    }
}