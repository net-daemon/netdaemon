using System.Runtime.InteropServices.ObjectiveC;
using System;
using NetDaemon.Common;
using Microsoft.Extensions.DependencyInjection;

namespace NetDaemon.Daemon.Tests.DaemonRunner.AppWithDiSetup;

[NetDaemonApp]
internal class RequireServiceApp
{
    public RequireServiceApp(ISomeService service)
    {
        Console.WriteLine("In App ctor");
        service.DoSomething();
    }
}

public interface ISomeService
{
    void DoSomething();
}

internal class ServiceImplementation : ISomeService
{
    private readonly Action<string> _logger;

    public ServiceImplementation(Action<string> logger)
    {
        _logger = logger;
    }

    public void DoSomething()
    {
        Console.WriteLine("In Service");
        _logger("Hello logger");
    }
}

public static class DiSetup
{
    [NetDaemon.Common.ServiceCollectionExtension]
    public static void UseSomeService(IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<ISomeService, ServiceImplementation>();
    }
}
