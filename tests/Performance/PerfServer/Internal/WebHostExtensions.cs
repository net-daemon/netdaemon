using Serilog;

namespace NetDaemon.Tests.Performance;

internal static class WebHostExtensions
{
    public static WebApplicationBuilder UsePerfServerSettings(this WebApplicationBuilder builder)
    {
        builder.Host
            .UseSerilog((context, provider, logConfig) =>
            {
                logConfig.ReadFrom.Configuration(context.Configuration);
            });

        builder.Services.AddScoped<PerfServerStartup>();

        return builder;
    }

    public static void AddPerfServer(this WebApplication app)
    {
        app.UseWebSockets();

        // NetDaemon is checking the state of input_boolean so let's fake a response
        app.MapGet("/api/states/{entityId}", (string entityId) =>
        {
            if (entityId.StartsWith("input_boolean.", StringComparison.Ordinal))
            {
                var id = entityId[14..];
                var inputBoolean = PerfServerStartup._inputBooleans.FirstOrDefault(n => n.Name == id);
                if (inputBoolean != null)
                {
                    return Results.Ok(new EntityState
                    {
                        EntityId = entityId,
                        State = "on",
                    });
                }
            }
            return Results.NotFound();
        });

        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/api/websocket")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var perfServer = context.RequestServices.GetRequiredService<PerfServerStartup>();
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await perfServer.ProcessWebsocket(webSocket);
                    return;
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }
            else
            {
                await next(context);
            }

        });
        app.UseRouting();
    }
}
