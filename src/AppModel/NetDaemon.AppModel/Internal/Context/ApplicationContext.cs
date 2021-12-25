using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
namespace NetDaemon.AppModel.Internal;

internal class ApplicationContext :
    IApplicationContext,
    IApplicationInstance,
    IAsyncDisposable
{
    public string Id { get; init; } = string.Empty;

    public bool IsEnabled { get; } = false;

    public Type Type { get; init; } = typeof(object);

    public object Instance { get; }

    private readonly IServiceScope? _serviceScope;
    public ApplicationContext(
        string id,
        Type type,
        IServiceProvider serviceProvider,
        object instance
    )
    {
        // Create a new ServiceScope for all objects we create for this app
        // this makes sure they will all be disposed along with the app
        _serviceScope = serviceProvider.CreateScope();
        serviceProvider = _serviceScope.ServiceProvider;

        var appScope = serviceProvider.GetService<ApplicationScope>();
        if (appScope != null)
        {
            appScope.ApplicationContext = this;
        }
        Id = id;
        Type = type;
        Instance = instance;
    }

    public async ValueTask DisposeAsync()
    {
        if (Instance is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }

        if (Instance is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (_serviceScope is IAsyncDisposable serviceScopeAsyncDisposable)
        {
            await serviceScopeAsyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        GC.SuppressFinalize(this);
    }
}
