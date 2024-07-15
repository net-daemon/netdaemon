namespace NetDaemon.Tests.Performance;

internal static class WebHostExtensions
{
    private static readonly WebSocketOptions _webSocketOptions = new()
    {
        KeepAliveInterval = TimeSpan.FromSeconds(120)
    };

    public static void UsePerfServerWebsockets(this WebApplication app)
    {
        app.Urls.Add("http://127.0.0.1:8002");
        app.UseWebSockets(_webSocketOptions);
        app.MapGet("/api/states/{entityId}", async (string entityId) =>
        {
            if (entityId.StartsWith("input_boolean."))
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
                else
                {
                    return Results.NotFound();
                }
            }
            else
            {
                return Results.NotFound();
            }
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
