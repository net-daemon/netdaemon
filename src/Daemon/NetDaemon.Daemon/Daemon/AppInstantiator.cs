using System;
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
                return new ApplicationContext(applicationType, appId, ServiceProvider);
            }
            catch (Exception e)
            {
                var message = $"Error instantiating app of type {applicationType} with id \"{appId}\"";

                _logger.LogTrace(e, $"{message}, use trace flag for details");
                _logger.LogError(message);
                throw new NetDaemonException(message, e);
            }
        }
    }
}