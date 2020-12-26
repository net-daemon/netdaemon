using System.Reflection;
using JoySoftware.HomeAssistant.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Daemon;
using NetDaemon.Daemon.Storage;
using NetDaemon.Service;
using Microsoft.Extensions.Options;
using System.IO;
using Microsoft.Extensions.Hosting;
using NetDaemon.Common;
using System;
using System.Net.WebSockets;
using Microsoft.AspNetCore.WebSockets;
using NetDaemon.Service.Api;
using NetDaemon.Common.Configuration;

namespace NetDaemon.Service
{
    public class ApiStartup
    {
        readonly bool _useAdmin = false;

        public ApiStartup(IConfiguration configuration)
        {
            Configuration = configuration;

            var enableAdminValue = Configuration.GetSection("NetDaemon").GetSection("Admin").Value;
            _ = bool.TryParse(enableAdminValue, out _useAdmin);
        }

        public IConfiguration Configuration { get; }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<RunnerService>();
            services.AddTransient<IHassClient, HassClient>();
            services.AddTransient<IDataRepository>(n => new DataRepository(
                Path.Combine(
                     n.GetRequiredService<IOptions<NetDaemonSettings>>().Value.GetAppSourceDirectory()
                    , ".storage")));
            services.AddTransient<IHttpHandler, NetDaemon.Daemon.HttpHandler>();
            services.AddSingleton<NetDaemonHost>();
            services.AddHttpClient();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (_useAdmin == true)
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                var webSocketOptions = new WebSocketOptions()
                {
                    KeepAliveInterval = TimeSpan.FromSeconds(120)
                    // ReceiveBufferSize = 4 * 1024
                };
                app.UseWebSockets(webSocketOptions);
                app.UseMiddleware<ApiWebsocketMiddleware>();
            }
        }
    }
}
