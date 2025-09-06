using Microsoft.Extensions.Hosting;
using NetDaemon.AppModel.Internal.AppFactories;

namespace NetDaemon.AppModel.Internal;

class FocusFilter
{
    private readonly ILogger<FocusFilter> _logger;
    private readonly IHostEnvironment? _hostEnvironment;

    public FocusFilter(ILogger<FocusFilter> logger, IHostEnvironment? hostEnvironment = null)
    {
        _logger = logger;
        _hostEnvironment = hostEnvironment;
    }

    public IReadOnlyCollection<IAppFactory> FilterFocusApps(IReadOnlyCollection<IAppFactory> allApps)
    {
        var focusApps = allApps.Where(a => a.HasFocus).ToList();

        if (focusApps.Count == 0) return allApps;

        foreach (var focusApp in focusApps)
        {
            _logger.LogInformation("[Focus] attribute is set for app {AppName}", focusApp.Id);
        }

        if (_hostEnvironment?.IsDevelopment() != true)
        {
            _logger.LogError("{Count} Focus apps were found but current environment is not 'Development', the [Focus] attribute is ignored" +
                             "Make sure the environment variable `DOTNET_ENVIRONMENT` is set to `Development` to use [Focus] or remove the [Focus] attribute when running in production", focusApps.Count);
            return allApps;
        }

        _logger.LogWarning("Found {AppCount} [Focus] apps, skipping all other apps", focusApps.Count);
        return focusApps;
    }
}
