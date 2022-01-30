namespace NetDaemon.AppModel.Internal.AppFactories;

internal interface IAppFactory
{
    object Create(IServiceProvider provider);

    string Id { get; }

    bool HasFocus { get; }
}