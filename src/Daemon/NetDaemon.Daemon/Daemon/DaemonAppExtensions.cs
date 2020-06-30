using JoySoftware.HomeAssistant.NetDaemon.Common;
using JoySoftware.HomeAssistant.NetDaemon.Common.Reactive;
using JoySoftware.HomeAssistant.NetDaemon.Daemon.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace JoySoftware.HomeAssistant.NetDaemon.Daemon
{
    public static class DaemonAppExtensions
    {
        public static async Task HandleAttributeInitialization(this INetDaemonAppBase netDaemonApp, INetDaemon _daemon)
        {
            var netDaemonAppType = netDaemonApp.GetType();
            foreach (var method in netDaemonAppType.GetMethods())
            {
                foreach (var attr in method.GetCustomAttributes(false))
                {
                    if (netDaemonApp is NetDaemonApp daemonApp)
                    {
                        switch (attr)
                        {
                            case HomeAssistantServiceCallAttribute hasstServiceCallAttribute:
                                await HandleServiceCallAttribute(_daemon, daemonApp, method, true).ConfigureAwait(false);
                                break;

                            case HomeAssistantStateChangedAttribute hassStateChangedAttribute:
                                HandleStateChangedAttribute(_daemon, hassStateChangedAttribute, daemonApp, method);
                                break;
                        }
                    }
                    else if (netDaemonApp is NetDaemonRxApp daemonRxApp)
                    {
                        switch (attr)
                        {
                            case HomeAssistantServiceCallAttribute hasstServiceCallAttribute:
                                await HandleServiceCallAttribute(_daemon, daemonRxApp, method, false).ConfigureAwait(false);
                                break;
                        }
                    }
                }
            }
        }

        private static (bool, string) CheckIfServiceCallSignatureIsOk(MethodInfo method, bool async)
        {
            if (async && method.ReturnType != typeof(Task))
                return (false, $"{method.Name} has not correct return type, expected Task");

            var parameters = method.GetParameters();

            if (parameters == null || (parameters != null && parameters.Length != 1))
                return (false, $"{method.Name} has not correct number of parameters");

            var dynParam = parameters![0];
            if (dynParam.CustomAttributes.Count() == 1 &&
                dynParam.CustomAttributes.First().AttributeType == typeof(DynamicAttribute))
                return (true, string.Empty);

            return (false, $"{method.Name} is not correct signature");
        }

        private static (bool, string) CheckIfStateChangedSignatureIsOk(MethodInfo method)
        {
            if (method.ReturnType != typeof(Task))
                return (false, $"{method.Name} has not correct return type, expected Task");

            var parameters = method.GetParameters();

            if (parameters == null || (parameters != null && parameters.Length != 3))
                return (false, $"{method.Name} has not correct number of parameters");

            if (parameters![0].ParameterType != typeof(string))
                return (false, $"{method.Name} first parameter exepected to be string for entityId");

            if (parameters![1].ParameterType != typeof(EntityState))
                return (false, $"{method.Name} second parameter exepected to be EntityState for toState");

            if (parameters![2].ParameterType != typeof(EntityState))
                return (false, $"{method.Name} first parameter exepected to be EntityState for fromState");

            return (true, string.Empty);
        }

        private static async Task HandleServiceCallAttribute(INetDaemon _daemon, NetDaemonAppBase netDaemonApp, MethodInfo method, bool async=true)
        {
            var (signatureOk, err) = CheckIfServiceCallSignatureIsOk(method, async);
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
        

        private static void HandleStateChangedAttribute(
                                    INetDaemon _daemon,
            HomeAssistantStateChangedAttribute hassStateChangedAttribute,
            NetDaemonApp netDaemonApp,
            MethodInfo method
            )
        {
            var (signatureOk, err) = CheckIfStateChangedSignatureIsOk(method);

            if (!signatureOk)
            {
                _daemon.Logger.LogWarning(err);
                return;
            }

            netDaemonApp.ListenState(hassStateChangedAttribute.EntityId,
            async (entityId, to, from) =>
            {
                try
                {
                    if (hassStateChangedAttribute.To != null)
                        if ((dynamic)hassStateChangedAttribute.To != to?.State)
                            return;

                    if (hassStateChangedAttribute.From != null)
                        if ((dynamic)hassStateChangedAttribute.From != from?.State)
                            return;

                    // If we don´t accept all changes in the state change
                    // and we do not have a state change so return
                    if (to?.State == from?.State && !hassStateChangedAttribute.AllChanges)
                        return;

                    await method.InvokeAsync(netDaemonApp, entityId, to!, from!).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _daemon.Logger.LogError(e, "Failed to invoke the ServiceCall function for app {appId}", netDaemonApp.Id);
                }
            });
        }
    }
}