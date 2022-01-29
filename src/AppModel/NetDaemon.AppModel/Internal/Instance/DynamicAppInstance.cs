using System.Reflection;

namespace NetDaemon.AppModel.Internal.Resolver;

internal class DynamicAppInstance : IAppInstance
{
    private readonly Type _type;

    public DynamicAppInstance(Type type)
    {
        _type = type;

        Id = AppInstanceHelper.GetAppId(type);
        HasFocus = AppInstanceHelper.GetAppFocus(type);
    }

    public string Id { get; }

    public bool HasFocus { get; }

    public object Create(IServiceProvider scopedServiceProvider)
    {
        return ActivatorUtilities.CreateInstance(scopedServiceProvider, _type);
    }


}