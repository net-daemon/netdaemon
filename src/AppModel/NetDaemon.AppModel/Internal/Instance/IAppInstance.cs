namespace NetDaemon.AppModel.Internal.Resolver;

internal interface IAppInstance
{
    string Id { get; }

    bool HasFocus { get; }

    object Create(IServiceProvider scopedServiceProvider);
}