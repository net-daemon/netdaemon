namespace NetDaemon.AppModel.Internal.AppFactories;

internal class FuncAppFactory(Delegate handler, string id, bool hasFocus) : IAppFactory
{
    public object? Create(IServiceProvider provider)
    {
        // Pass any arguments to the delegate method by resolving them from the service provider
        var args = handler.Method.GetParameters().Select(p => provider.GetService(p.ParameterType)).ToArray();
        return handler.DynamicInvoke(args);
    }

    public string Id { get; } = id;

    public bool HasFocus { get; } = hasFocus;
}
