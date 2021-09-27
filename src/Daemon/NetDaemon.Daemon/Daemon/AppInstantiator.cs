using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;
using NetDaemon.Common.Exceptions;

namespace NetDaemon.Daemon
{
    internal class AppInstantiator : IAppInstantiator
    {
        private readonly ILogger _logger;
        public IServiceProvider ServiceProvider { get; }

        public AppInstantiator(IServiceProvider serviceProvider, ILogger logger)
        {
            _logger = logger;
            ServiceProvider = serviceProvider;
        }

        public ApplicationContext Instantiate(Type applicationType, string appId)
        {
            try
            {
                return ApplicationContext.Create(applicationType, appId, ServiceProvider, ServiceProvider.GetRequiredService<INetDaemon>());
            }
            catch (Exception e)
            {
                var message = $"Error instantiating app of type {applicationType} with id \"{appId}\"";

                _logger.LogTrace(e, message);
                _logger.LogError($"{message}, use trace flag for details");
                throw new NetDaemonException(message, e);
            }
        }
    }
}