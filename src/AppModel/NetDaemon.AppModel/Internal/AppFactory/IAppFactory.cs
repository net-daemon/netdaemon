namespace NetDaemon.AppModel.Internal.AppFactory;

internal interface IAppFactory
{
    object Create(IServiceProvider provider);

    string Id { get; }

    bool HasFocus { get; }
}