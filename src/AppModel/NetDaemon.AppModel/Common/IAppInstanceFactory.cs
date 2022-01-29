namespace NetDaemon.AppModel;

public interface IAppInstanceFactory
{
    object Create(IServiceProvider scopedServiceProvider, Type appType);
}