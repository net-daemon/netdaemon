using System.Reflection;
using JoySoftware.HomeAssistant.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Daemon;
using NetDaemon.Daemon.Storage;
using NetDaemon.Service.Configuration;
using NetDaemon.Service;
using Microsoft.Extensions.Options;
using System.IO;
using Microsoft.Extensions.Hosting;
using NetDaemon.Common;

namespace NetDaemon.Service
{
    public class ApiStartup
    {
        bool _useAdmin = false;

        public ApiStartup(IConfiguration configuration)
        {
            Configuration = configuration;

            var enableAdminValue = Configuration.GetSection("NetDaemon").GetSection("Admin").Value;
            bool.TryParse(enableAdminValue, out _useAdmin);
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // services.Configure<HomeAssistantSettings>(Context.Configuration.GetSection("HomeAssistant"));
            // services.Configure<NetDaemonSettings>(context.Configuration.GetSection("NetDaemon"));
            services.AddHostedService<RunnerService>();
            services.AddTransient<IHassClient, HassClient>();
            services.AddTransient<IDataRepository>(n => new DataRepository(Path.Combine(n.GetRequiredService<IOptions<NetDaemonSettings>>().Value.SourceFolder!, ".storage")));
            services.AddTransient<IHttpHandler, NetDaemon.Daemon.HttpHandler>();
            services.AddSingleton<NetDaemonHost>();
            services.AddHttpClient();


            if (_useAdmin == true)
            {
                // Only enable them if 
                services.AddControllers().PartManager.ApplicationParts.Add(new AssemblyPart(Assembly.GetExecutingAssembly()));
                services.AddRouting();
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (_useAdmin == true)
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                // app.UseHttpsRedirection();

                app.UseRouting();

                // app.UseAuthorization();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            }
        }
    }
}
