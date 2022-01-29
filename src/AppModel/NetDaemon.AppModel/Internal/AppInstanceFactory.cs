namespace NetDaemon.AppModel.Internal;

internal class AppInstanceFactory : IAppInstanceFactory
{
    public object Create(IServiceProvider scopedServiceProvider, Type appType)
    {
        return ActivatorUtilities.CreateInstance(scopedServiceProvider, appType);
    }
}