using Microsoft.Extensions.DependencyInjection;

namespace NetDaemon.Service.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNetDaemon(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddHostedService<RunnerService>();

            return services;
        }
    }
}