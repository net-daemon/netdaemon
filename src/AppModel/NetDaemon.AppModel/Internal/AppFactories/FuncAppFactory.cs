using System.Reflection;

namespace NetDaemon.AppModel.Internal.AppFactories;

internal class FuncAppFactory : IAppFactory
{
    private readonly Func<IServiceProvider, object?> _func;

    private FuncAppFactory(Func<IServiceProvider, object?> func, Type type, string? id, bool? focus)
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

    public object? Create(IServiceProvider provider)
    {
        return _func.Invoke(provider);
    }

    public string Id { get; }

    public bool HasFocus { get; }

    // TODO: Remove
    public static FuncAppFactory Create<TAppType>(Func<IServiceProvider, TAppType> func,
        string? id = default, bool? focus = default) where TAppType : class
    {
        return new FuncAppFactory(func, typeof(TAppType), id, focus);
    }

    public static FuncAppFactory Create(Delegate @delegate, string? id = null, bool hasFocus = false)
    {
        return new FuncAppFactory(sp => ResolveDelegateParamsFromServiceProvider(@delegate, sp),
            @delegate.Method.ReturnType, // TODO: do not base id and focus on the return type for delegate apps
            id, hasFocus);
    }

    private static object? ResolveDelegateParamsFromServiceProvider(Delegate @delegate, IServiceProvider provider)
    {
        // Pass any arguments to the delegate method by resolving them from the service provider
        var args = @delegate.Method.GetParameters().Select(p => provider.GetService(p.ParameterType)).ToArray();
        return @delegate.DynamicInvoke(args);
    }

    public static FuncAppFactory Create(Type type, string? id = default, bool? focus = default)
    {
        return new FuncAppFactory(CreateFactoryFunc(type), type, id, focus);
    }

    private static Func<IServiceProvider, object> CreateFactoryFunc(Type type)
    {
        return provider => ActivatorUtilities.CreateInstance(provider, type);
    }
}
