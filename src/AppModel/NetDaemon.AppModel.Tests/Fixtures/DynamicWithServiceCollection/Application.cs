using NetDaemon.AppModel;
using Microsoft.Extensions.DependencyInjection;
namespace Apps;

public static class ServiceCollectionRegister
{
    [ServiceCollectionExtension]
    public static void RegisterSomeGreatServices(IServiceCollection services)
    {
        services.AddSingleton<InjectMePlease>();
    }
}

public class InjectMePlease
{
    public InjectMePlease()
    {
        
    }
    
    public string AValue { get; init; } = "SomeInjectedValue";
}

[NetDaemonApp]
public class InjectedApp
{
    public string InjectedValue { get; set; }
    public InjectedApp(InjectMePlease injectedClass)
    {
        InjectedValue = injectedClass.AValue;
    }
}