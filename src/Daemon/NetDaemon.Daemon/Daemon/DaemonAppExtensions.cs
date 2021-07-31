using System;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;
using NetDaemon.Common.Exceptions;
using NetDaemon.Common.Reactive;
using NetDaemon.Daemon.Config;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.Daemon
{
    public static class DaemonAppExtensions
    {
        public static async Task HandleAttributeInitialization(this INetDaemonApp netDaemonApp, INetDaemon daemon)
        {
            _ = daemon ??
               throw new NetDaemonArgumentNullException(nameof(daemon));
            _ = netDaemonApp ??
               throw new NetDaemonArgumentNullException(nameof(netDaemonApp));

            var netDaemonAppType = netDaemonApp.GetType();

            foreach (var method in netDaemonAppType.GetMethods())
            {
                foreach (var attr in method.GetCustomAttributes(false))
                {
                    if (netDaemonApp is NetDaemonRxApp daemonRxApp)
                    {
                        switch (attr)
                        {
                            case HomeAssistantServiceCallAttribute:
                                await HandleServiceCallAttribute(daemon, daemonRxApp, method, false).ConfigureAwait(false);
                                break;
                        }
                    }
                }
            }
        }

        private static (bool, string) CheckIfServiceCallSignatureIsOk(MethodInfo method, bool isAsync)
        {
            if (isAsync && method.ReturnType != typeof(Task))
                return (false, $"{method.Name} has not correct return type, expected Task");

            var parameters = method.GetParameters();

            if (parameters == null || parameters.Length != 1)
                return (false, $"{method.Name} has not correct number of parameters");

            var dynParam = parameters![0];
            if (dynParam.CustomAttributes.Count() == 1 &&
                dynParam.CustomAttributes.First().AttributeType == typeof(DynamicAttribute))
            {
                return (true, string.Empty);
            }

            return (false, $"{method.Name} is not correct signature");
        }

        [SuppressMessage("", "CA1031")]
        private static async Task HandleServiceCallAttribute(INetDaemon _daemon, NetDaemonAppBase netDaemonApp, MethodInfo method, bool isAsync = true)
        {
            var (signatureOk, err) = CheckIfServiceCallSignatureIsOk(method, isAsync);
            if (!signatureOk)
            {
                _daemon.Logger.LogWarning(err);
                return;
            }

            dynamic serviceData = new FluentExpandoObject();
            serviceData.method = method.Name;
            serviceData.@class = netDaemonApp.GetType().Name;
            await _daemon.CallServiceAsync("netdaemon", "register_service", serviceData).ConfigureAwait(false);

            netDaemonApp.ListenServiceCall("netdaemon", $"{serviceData.@class}_{serviceData.method}",
                async (data) =>
                {
                    try
                    {
                        var expObject = data as ExpandoObject;

                        await method.InvokeAsync(netDaemonApp, expObject!).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        _daemon.Logger.LogError(e, "Failed to invoke the ServiceCall function for app {appId}", netDaemonApp);
                    }
                });
        }
    }
}