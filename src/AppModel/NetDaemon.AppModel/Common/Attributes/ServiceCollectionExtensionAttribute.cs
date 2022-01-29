namespace NetDaemon.AppModel;

/// <summary>
///     Indicates method in dynamically compile code should be called as a ServiceCollectionExtension to setup services
/// </summary>
/// <remarks>
///     The method should have `public static void RegisterServices(IServiceCollection services){}`
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ServiceCollectionExtensionAttribute : Attribute
{
}
