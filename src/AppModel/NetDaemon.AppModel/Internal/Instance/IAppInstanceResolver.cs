namespace NetDaemon.AppModel.Internal.Resolver;

internal interface IAppInstanceResolver
{
    IReadOnlyCollection<IAppInstance> GetApps();
}