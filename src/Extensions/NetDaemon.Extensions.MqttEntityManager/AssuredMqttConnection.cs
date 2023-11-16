using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using NetDaemon.Extensions.MqttEntityManager.Exceptions;
using NetDaemon.Extensions.MqttEntityManager.Helpers;
using NetDaemon.Models;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Wrapper to assure an MQTT connection
/// </summary>
internal class AssuredMqttConnection : IAssuredMqttConnection, IDisposable
{
    private readonly ILogger<AssuredMqttConnection> _logger;
    private readonly Task _connectionTask;
    private IManagedMqttClient? _mqttClient;
    private bool _disposed;

    /// <summary>
    /// Wrapper to assure an MQTT connection
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mqttFactory"></param>
    /// <param name="mqttConfig"></param>
    public AssuredMqttConnection(
        ILogger<AssuredMqttConnection> logger,
        IMqttFactoryWrapper mqttFactory,
        IOptions<MqttConfiguration> mqttConfig)
    {
        _logger = logger;

        _logger.LogTrace("MQTT initiating connection");
        _connectionTask = Task.Run(() => ConnectAsync(mqttConfig.Value, mqttFactory));
    }

    /// <summary>
    /// Ensures that the MQTT client is available
    /// </summary>
    /// <returns></returns>
    /// <exception cref="MqttConnectionException">Timed out while waiting for connection</exception>
    public async Task<IManagedMqttClient> GetClientAsync()
    {
        await _connectionTask;

        return _mqttClient ?? throw new MqttConnectionException("Unable to create MQTT connection");
    }

    private async Task<MqttConfiguration?> RequestMqttConfiguration()
    {
        try
        {
            var hassToken = Environment.GetEnvironmentVariable("HASS_TOKEN");
            if (hassToken != null)
            {
                _logger.LogTrace("Querying Home Assitant Supervisor for MQTT service configuration");
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", hassToken);
                var responseStream = await httpClient.GetStreamAsync("http://supervisor/services/mqtt").ConfigureAwait(false);
                var response = await JsonSerializer.DeserializeAsync<HASupervisorResult<MqttConfiguration>>(responseStream).ConfigureAwait(false);
                if (response?.Result.Equals("ok", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return response.Data;
                }
            }
        }
        catch
        {
            _logger.LogTrace("Unable to get MQTT configuration from Home Assistant");
        }
        return null;
    }

    private async Task ConnectAsync(MqttConfiguration mqttConfig, IMqttFactoryWrapper mqttFactory)
    {
        if (string.IsNullOrEmpty(mqttConfig.Host))
        {
            var newConfig = await RequestMqttConfiguration();
            if (newConfig != null)
            {
                mqttConfig = newConfig;
            }
        }

        _logger.LogTrace("Connecting to MQTT broker at {Host}:{Port}/{UserName}",
            mqttConfig.Host, mqttConfig.Port, mqttConfig.UserName);

        var clientOptions = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithTcpServer(mqttConfig.Host, mqttConfig.Port)
                .WithCredentials(mqttConfig.UserName, mqttConfig.Password))
            .Build();

        _mqttClient = mqttFactory.CreateManagedMqttClient();

        _mqttClient.ConnectedAsync += MqttClientOnConnectedAsync;
        _mqttClient.DisconnectedAsync += MqttClientOnDisconnectedAsync;

        await _mqttClient.StartAsync(clientOptions);

        _logger.LogTrace("MQTT client is ready");
    }

    private Task MqttClientOnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        _logger.LogDebug("MQTT disconnected: {Reason}", BuildErrorResponse(arg));
        return Task.CompletedTask;
    }

    private Task MqttClientOnConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        _logger.LogDebug("MQTT connected: {ResultCode}", arg.ConnectResult.ResultCode);
        return Task.CompletedTask;
    }

    private static string BuildErrorResponse(MqttClientDisconnectedEventArgs arg)
    {
        var sb = new StringBuilder();

        sb.AppendLine(CultureInfo.InvariantCulture, $"{arg.Exception?.Message} ({arg.Reason})");     // Note: arg.ReasonString is always null
        var ex = arg.Exception?.InnerException;
        while (ex != null)
        {
            sb.AppendLine(ex.Message);
            ex = ex.InnerException;
        }

        return sb.ToString();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _logger.LogTrace("MQTT disconnecting");
        _connectionTask?.Dispose();
        _mqttClient?.Dispose();
    }
}
