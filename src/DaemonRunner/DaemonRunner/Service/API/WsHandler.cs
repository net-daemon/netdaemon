using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
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
using NetDaemon.Common.Exceptions;
using NetDaemon.Daemon;

namespace NetDaemon.Service.Api
{
    public class ApiWebsocketMiddleware
    {
        private static readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

        private readonly RequestDelegate _next;

        private readonly ILogger<ApiWebsocketMiddleware> _logger;
        private readonly NetDaemonSettings? _netdaemonSettings;
        private readonly HomeAssistantSettings? _homeassistantSettings;

        private readonly NetDaemonHost? _host;

        private readonly JsonSerializerOptions _jsonOptions = new()
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
            _ = netDaemonSettings ??
               throw new NetDaemonArgumentNullException(nameof(netDaemonSettings));
            _ = homeAssistantSettings ??
               throw new NetDaemonArgumentNullException(nameof(homeAssistantSettings));
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
                    Data = _host?.AllAppContexts.Select(n => new ApiApplication()
                    {
                        Id = n.Id,
                        Dependencies = n.Dependencies,
                        IsEnabled = n.IsEnabled,
                        Description = n.Description,
                        NextScheduledEvent = n.IsEnabled ? n.RuntimeInfo.NextScheduledEvent : null,
                        LastErrorMessage = n.IsEnabled ? n.RuntimeInfo.LastErrorMessage : null
                    })
                };
                await BroadCast(JsonSerializer.Serialize(eventMessage, _jsonOptions)).ConfigureAwait(false);
            }
        }
        
        [SuppressMessage("", "CA1031")]
        public async Task Invoke(HttpContext context)
        {
            _ = context ??
               throw new NetDaemonArgumentNullException(nameof(context));
            if (!context.WebSockets.IsWebSocketRequest && context.Request.Path != "/api/ws")
            {
                await _next.Invoke(context).ConfigureAwait(false);
                return;
            }

            CancellationToken ct = context.RequestAborted;
            WebSocket? currentSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
            var socketId = Guid.NewGuid().ToString();

            _sockets.TryAdd(socketId, currentSocket);
            _logger.LogDebug("New websocket client {socketId}", socketId);
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var msg = await GetNextMessageAsync(currentSocket, ct).ConfigureAwait(false);

                    if (currentSocket.State != WebSocketState.Open)
                    {
                        break;
                    }

                    if (msg is not null)
                    {
                        switch (msg.Type)
                        {
                            case "apps":

                                var eventMessage = new WsExternalEvent
                                {
                                    Type = "apps",
                                    Data = _host?.AllAppContexts.Select(n => new ApiApplication()
                                    {
                                        Id = n.Id,
                                        Dependencies = n.Dependencies,
                                        IsEnabled = n.IsEnabled,
                                        Description = n.Description,
                                        NextScheduledEvent = n.IsEnabled ? n.RuntimeInfo.NextScheduledEvent : null,
                                        LastErrorMessage = n.IsEnabled ? n.RuntimeInfo.LastErrorMessage : null
                                    })
                                };

                                await BroadCast(JsonSerializer.Serialize(eventMessage, _jsonOptions)).ConfigureAwait(false);

                                break;
                            case "settings":
                                var tempResult = new ApiConfig
                                {
                                    DaemonSettings = _netdaemonSettings,
                                    HomeAssistantSettings = _homeassistantSettings
                                };

                                // For first release we do not expose the token
                                if (tempResult.HomeAssistantSettings is not null)
                                {
                                    tempResult.HomeAssistantSettings.Token = "";
                                }
                                var settingsMessage = new WsExternalEvent
                                {
                                    Type = "settings",
                                    Data = tempResult
                                };

                                await BroadCast(JsonSerializer.Serialize(settingsMessage, _jsonOptions)).ConfigureAwait(false);
                                break;

                            case "app":

                                if (msg.App is null)
                                {
                                    _logger.LogDebug("App should not bee null.");
                                    continue;
                                }
                                if (msg.ServiceData is not null)
                                {
                                    var command = JsonSerializer.Deserialize<WsAppCommand>(msg.ServiceData.Value.GetRawText(), _jsonOptions);
                                    if (command is null)
                                    {
                                        _logger.LogDebug("Failed to read command.");
                                        continue;
                                    }
                                    if (command.IsEnabled is not null)
                                    {
                                        if (command.IsEnabled.Value)
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
                _logger.LogTrace(e, "Unhandled error in websocket communication");
            }

            _sockets.TryRemove(socketId, out _);

            if (
                currentSocket.State == WebSocketState.Open ||
                currentSocket.State == WebSocketState.CloseReceived ||
                currentSocket.State == WebSocketState.CloseSent)
            {
                await currentSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct).ConfigureAwait(false);
            }
            currentSocket.Dispose();
        }

        public async Task BroadCast(string message, CancellationToken ct = default)
        {
            _logger.LogTrace("Broadcasting to {count} clients", _sockets.Count);
            foreach (var socket in _sockets)
            {
                if (socket.Value.State != WebSocketState.Open)
                {
                    continue;
                }

                await SendStringAsync(socket.Value, message, ct).ConfigureAwait(false);
            }
        }

        private static Task SendStringAsync(WebSocket socket, string data, CancellationToken ct = default)
        {
            var buffer = Encoding.UTF8.GetBytes(data);
            var segment = new ArraySegment<byte>(buffer);
            return socket.SendAsync(segment, WebSocketMessageType.Text, true, ct);
        }

        private static async Task<WsMessage?> GetNextMessageAsync(WebSocket socket, CancellationToken ct = default)
        {
            // System.Text.Json.JsonSerializer.DeserializeAsync(socket.)
            var buffer = new ArraySegment<byte>(new byte[8192]);
            _ = buffer.Array ?? throw new NetDaemonNullReferenceException("Failed to allocate memory buffer");

            using var ms = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                ct.ThrowIfCancellationRequested();

                result = await socket.ReceiveAsync(buffer, ct).ConfigureAwait(false);
                if (!result.CloseStatus.HasValue)
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                else
                    return null;
            }
            while (!result.EndOfMessage);

            ms.Seek(0, SeekOrigin.Begin);
            if (result.MessageType != WebSocketMessageType.Text)
            {
                return null;
            }

            using var reader = new StreamReader(ms, Encoding.UTF8);
            var msgString = await reader.ReadToEndAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<WsMessage>(msgString);
        }
    }
}