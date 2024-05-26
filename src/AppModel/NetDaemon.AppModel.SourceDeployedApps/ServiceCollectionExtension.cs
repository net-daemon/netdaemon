using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.AppModel.Internal.AppAssemblyProviders;
using NetDaemon.AppModel.Internal.Compiler;

namespace NetDaemon.AppModel;

/// <summary>
///     ServiceCollection extensions
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Add apps from c# source code using the configuration source to find path
    /// </summary>
    /// <param name="services">Services</param>
    /// <param name="useDebug">Override UseDebug in CompileSettings</param>
    public static IServiceCollection AddAppsFromSource(this IServiceCollection services, bool useDebug = false)
    {
        // // We make sure we only add AppModel services once
         services
            .AddNetDaemonAppModel()
            .AddSingleton<Compiler>()
            .AddSingleton<ICompiler>(s => s.GetRequiredService<Compiler>())
            .AddSingleton<SyntaxTreeResolver>()
            .AddSingleton<ISyntaxTreeResolver>(s => s.GetRequiredService<SyntaxTreeResolver>())
            .AddOptions<CompileSettings>().Configure(settings => settings.UseDebug = useDebug);

        // We need to compile it here so we can dynamically add the service providers
        var assemblyResolver = ActivatorUtilities.CreateInstance<DynamicallyCompiledAppAssemblyProvider>(services.BuildServiceProvider());
        services.RegisterDynamicFunctions(assemblyResolver.GetAppAssembly());

        // And now register the assembly resolver that will have the assembly already compiled
        services.AddSingleton(assemblyResolver);
        services.AddSingleton<IAppAssemblyProvider>(s => s.GetRequiredService<DynamicallyCompiledAppAssemblyProvider>());

        return services;
    }

    private static IServiceCollection RegisterDynamicFunctions(this IServiceCollection services, Assembly assembly)
    {
        var methods = assembly.GetTypes().SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
                          .Where(m => m.GetCustomAttribute<ServiceCollectionExtensionAttribute>() != null).ToArray() ??
                      [];

        if (methods.Any(
                m => m.GetParameters().Length != 1 && m.GetParameters()[0].GetType() != typeof(IServiceProvider)))
            throw new InvalidOperationException(
                "Methods with [ServiceCollectionExtension] Attribute should have exactly one parameter of type IServiceCollection");

        foreach (var methodInfo in methods) methodInfo.Invoke(null, [services]);

        return services;
    }
}
