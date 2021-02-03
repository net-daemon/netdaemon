
using System;
using JoySoftware.HomeAssistant.Client;
using NetDaemon.Common.Exceptions;

namespace NetDaemon.Daemon
{
    /// <summary>
    ///     Factory to instance HassClient
    /// </summary>
    public interface IHassClientFactory
    {
        /// <summary>
        ///     Instance a new HassClient
        /// </summary>
        IHassClient? New();
    }

    public class HassClientFactory : IHassClientFactory
    {
        readonly IServiceProvider _serviceProvider;
        public HassClientFactory(IServiceProvider? serviceProvider = null)
        {
            _serviceProvider = serviceProvider ?? throw new NetDaemonArgumentNullException(nameof(serviceProvider));
        }
        public IHassClient? New()
        {
            return _serviceProvider.GetService(typeof(IHassClient)) as IHassClient;
        }
    }
}