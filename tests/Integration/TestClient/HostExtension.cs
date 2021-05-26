using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.Common.Exceptions;

namespace TestClient
{
    public static class NetDaemonExtensions
    {
        /// <summary>
        ///     Adds the integration tests as a service
        /// </summary>
        /// <param name="hostBuilder">The current host builder instance</param>
        public static IHostBuilder UseIntegrationTest(this IHostBuilder hostBuilder)
        {
            _ = hostBuilder ??
               throw new NetDaemonArgumentNullException(nameof(hostBuilder));

            return hostBuilder
                .ConfigureServices((_, services) => services.AddHostedService<IntegrationTestsService>());
        }
    }
}