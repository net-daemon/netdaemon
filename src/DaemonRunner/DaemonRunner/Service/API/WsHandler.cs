using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetDaemon.Common;
using NetDaemon.Common.Configuration;
using NetDaemon.Daemon;

namespace NetDaemon.Service.Api
{

    public class ApiWebsocketMiddleware
    {
        private static ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        private readonly RequestDelegate _next;

        private readonly ILogger<ApiWebsocketMiddleware> _logger;
        private readonly NetDaemonSettings? _netdaemonSettings;
        private readonly HomeAssistantSettings? _homeassistantSettings;

        private readonly NetDaemonHost? _host;

        private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        public ApiWebsocketMiddleware(
            RequestDelegate next,
            IOptions<NetDaemonSettings> netDaemonSettings,
            IOptions<HomeAssistantSettings> homeAssistantSettings,
            ILoggerFactory? loggerFactory = null,
            NetDaemonHost? host = null
            )
        {
            _logger = loggerFactory.CreateLogger<ApiWebsocketMiddleware>();
            _host = host;
            _netdaemonSettings = netDaemonSettings.Value;
            _homeassistantSettings = homeAssistantSettings.Value;
            _next = next;
            _host?.SubscribeToExternalEvents(NewEvent);
        }

        private async Task NewEvent(ExternalEventBase ev)
        {
            if (ev is AppsInformationEvent appEvent)
            {
                var eventMessage = new WsExternalEvent
                {
                    Type = "apps",
                    Data = _host?.AllAppInstances.Select(n => new ApiApplication()
                    {
                        Id = n.Id,
                        Dependencies = n.Dependencies,
                        IsEnabled = n.IsEnabled,
                        Description = n.Description,
                        NextScheduledEvent = n.IsEnabled ? n.RuntimeInfo.NextScheduledEvent : null,
                        LastErrorMessage = n.IsEnabled ? n.RuntimeInfo.LastErrorMessage : null
                    })
                };
                await BroadCast(JsonSerializer.Serialize<WsExternalEvent>(eventMessage, _jsonOptions));
            }
        }
        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest && context.Request.Path != "/api/ws")
            {
                await _next.Invoke(context);
                return;
            }

            CancellationToken ct = context.RequestAborted;
            WebSocket? currentSocket = await context.WebSockets.AcceptWebSocketAsync();
            var socketId = Guid.NewGuid().ToString();

            _sockets.TryAdd(socketId, currentSocket);
            _logger.LogDebug("New websocket client {socketId}", socketId);
            try
            {

                while (true)
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }

                    var msg = await GetNextMessageAsync(currentSocket, ct);

                    if (currentSocket.State != WebSocketState.Open)
                    {
                        break;
                    }

                    if (msg is object)
                    {
                        switch (msg.Type)
                        {
                            case "apps":
                                var apps = _host?.AllAppInstances.Select(n => new ApiApplication()
                                {
                                    Id = n.Id,
                                    Dependencies = n.Dependencies,
                                    IsEnabled = n.IsEnabled,
                                    Description = n.Description,
                                    NextScheduledEvent = n.IsEnabled ? n.RuntimeInfo.NextScheduledEvent : null,
                                    LastErrorMessage = n.IsEnabled ? n.RuntimeInfo.LastErrorMessage : null
                                });

                                await SendStringAsync(currentSocket, JsonSerializer.Serialize<IEnumerable<ApiApplication>>(apps ?? new ApiApplication[] { }, _jsonOptions), ct);

                                break;
                            case "settings":
                                var tempResult = new ApiConfig
                                {
                                    DaemonSettings = _netdaemonSettings,
                                    HomeAssistantSettings = _homeassistantSettings
                                };
                                // For first release we do not expose the token
                                if (tempResult.HomeAssistantSettings is object)
                                {
                                    tempResult.HomeAssistantSettings.Token = "";
                                }
                                await SendStringAsync(currentSocket, JsonSerializer.Serialize<ApiConfig>(tempResult, _jsonOptions), ct);
                                break;

                            case "app":

                                if (msg.App is null)
                                {
                                    _logger.LogDebug("App should not bee null.");
                                    continue;
                                }
                                if (msg.ServiceData is object)
                                {
                                    var command = JsonSerializer.Deserialize<WsAppCommand>(msg.ServiceData.Value.GetRawText(), _jsonOptions);
                                    if (command is null)
                                    {
                                        _logger.LogDebug("Failed to read command.");
                                        continue;
                                    }
                                    if (command.IsEnabled is object)
                                    {
                                        if (command.IsEnabled ?? false)
                                        {
                                            _host?.CallService("switch", "turn_on", new { entity_id = $"switch.netdaemon_{msg.App.ToSafeHomeAssistantEntityId()}" });
                                        }
                                        else
                                        {
                                            _host?.CallService("switch", "turn_off", new { entity_id = $"switch.netdaemon_{msg.App.ToSafeHomeAssistantEntityId()}" });
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "WEBSOCKET PROBLEM");
            }

            _sockets.TryRemove(socketId, out _);

            await currentSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
            currentSocket.Dispose();
        }

        public async Task BroadCast(string message, CancellationToken ct = default(CancellationToken))
        {
            _logger.LogInformation("Broadcasting to {count} clients", _sockets.Count);
            foreach (var socket in _sockets)
            {
                if (socket.Value.State != WebSocketState.Open)
                {
                    continue;
                }

                await SendStringAsync(socket.Value, message, ct);
            }
        }

        private static Task SendStringAsync(WebSocket socket, string data, CancellationToken ct = default(CancellationToken))
        {
            var buffer = Encoding.UTF8.GetBytes(data);
            var segment = new ArraySegment<byte>(buffer);
            return socket.SendAsync(segment, WebSocketMessageType.Text, true, ct);
        }

        private static async Task<WsMessage?> GetNextMessageAsync(WebSocket socket, CancellationToken ct = default(CancellationToken))
        {
            // System.Text.Json.JsonSerializer.DeserializeAsync(socket.)
            var buffer = new ArraySegment<byte>(new byte[8192]);
            _ = buffer.Array ?? throw new NullReferenceException("Failed to allocate memory buffer");

            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    ct.ThrowIfCancellationRequested();

                    result = await socket.ReceiveAsync(buffer, ct);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                if (result.MessageType != WebSocketMessageType.Text)
                {
                    return null;
                }

                using (var reader = new StreamReader(ms, Encoding.UTF8))
                {
                    var msgString = await reader.ReadToEndAsync();
                    return JsonSerializer.Deserialize<WsMessage>(msgString);
                }
            }
        }

    }
}