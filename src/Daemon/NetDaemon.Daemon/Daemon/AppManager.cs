using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;

namespace NetDaemon.Daemon
{
    internal class AppManager
    {
        private readonly IServiceProvider _serviceProvider;

        public AppManager( IServiceProvider serviceProvider, ILogger logger)
        {
            _serviceProvider = serviceProvider;
            Logger = logger;
        }

        public ILogger Logger { get; }

        public ConcurrentDictionary<string, ApplicationContext> InternalAllAppInstances { get; } = new();

        public ConcurrentDictionary<string, ApplicationContext> InternalRunningAppInstances { get; } = new();

        /// <inheritdoc/>
        public async Task ReloadAllApps(IInstanceDaemonApp appInstanceManager)
        {
            Logger.LogTrace("Loading all apps ({Instances}, {Running})", InternalAllAppInstances.Count,
                InternalRunningAppInstances.Count);

            // First unload any apps running
            await UnloadAllApps().ConfigureAwait(false);
            
            // Get all instances
            var applicationContexts = appInstanceManager.InstanceDaemonApps(_serviceProvider!);

            if (!InternalRunningAppInstances.IsEmpty)
            {
                Logger.LogWarning("Old apps not unloaded correctly. {Nr} apps still loaded.",
                    InternalRunningAppInstances.Count);
                InternalRunningAppInstances.Clear();
            }

            foreach (var applicationContext in applicationContexts)
            {
                InternalAllAppInstances[applicationContext.Id!] = applicationContext;
                if (await RestoreAppState(applicationContext).ConfigureAwait(false))
                {
                    InternalRunningAppInstances[applicationContext.Id!] = applicationContext;
                }
            }

            // Now run initialize on all sorted by dependencies
            var orderedApps = AppSorter.SortByDependency(InternalRunningAppInstances.Values.ToList());
            foreach (var applicationContext in orderedApps)
            {
                await InitializeApp(applicationContext).ConfigureAwait(false);
            }
        }

        private async Task InitializeApp(ApplicationContext applicationContext)
        {
            applicationContext.Start();

            // Init by calling the InitializeAsync
            var taskInitAsync = applicationContext.InitializeAsync();
            var taskAwaitedAsyncTask = await Task.WhenAny(taskInitAsync, Task.Delay(5000)).ConfigureAwait(false);
            if (taskAwaitedAsyncTask != taskInitAsync)
            {
                Logger.LogWarning(
                    "InitializeAsync of application {App} took longer that 5 seconds, make sure InitializeAsync is not blocking!",
                    applicationContext.Id);
            }

            Logger.LogInformation("Successfully loaded app {AppId} ({Class})", applicationContext.Id, applicationContext.ApplicationInstance?.GetType().Name);
        }

        [SuppressMessage("", "CA1031")]
        private async Task<bool> RestoreAppState(ApplicationContext appContext)
        {
            try
            {
                await appContext.RestoreStateAsync().ConfigureAwait(false);
                return appContext.IsEnabled;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to load app {AppId}", appContext.Id);
                return false;
            }
        }

        /// <inheritdoc/>
        [SuppressMessage("", "CA1031")]
        public async Task UnloadAllApps()
        {
            Logger.LogTrace("Unloading all apps ({Instances}, {Running})", InternalAllAppInstances.Count,
                InternalRunningAppInstances.Count);
            foreach (var app in InternalAllAppInstances.Values)
            {
                await app.DisposeAsync().ConfigureAwait(false);
            }

            InternalAllAppInstances.Clear();
            InternalRunningAppInstances.Clear();
        }
    }
}