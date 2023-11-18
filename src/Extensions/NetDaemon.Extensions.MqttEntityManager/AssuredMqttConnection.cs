using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using NetDaemon.Extensions.MqttEntityManager.Exceptions;
using NetDaemon.Extensions.MqttEntityManager.Helpers;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Wrapper to assure an MQTT connection
/// </summary>
internal class AssuredMqttConnection : IAssuredMqttConnection, IDisposable
{
    private readonly ILogger<AssuredMqttConnection> _logger;
    private readonly IMqttClientOptionsFactory _mqttClientOptionsFactory;
    private readonly Task _connectionTask;
    private IManagedMqttClient? _mqttClient;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssuredMqttConnection"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="mqttClientOptionsFactory">The MQTT client options factory.</param>
    /// <param name="mqttFactory">The MQTT factory wrapper.</param>
    /// <param name="mqttConfig">The MQTT configuration.</param>
    public AssuredMqttConnection(
        ILogger<AssuredMqttConnection> logger,
        IMqttClientOptionsFactory mqttClientOptionsFactory,
        IMqttFactoryWrapper mqttFactory,
        IOptions<MqttConfiguration> mqttConfig)
    {
        _logger = logger;
        _mqttClientOptionsFactory = mqttClientOptionsFactory;
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

    private async Task ConnectAsync(MqttConfiguration mqttConfig, IMqttFactoryWrapper mqttFactory)
    {
        _logger.LogTrace("Connecting to MQTT broker at {Host}:{Port}/{UserName}",
            mqttConfig.Host, mqttConfig.Port, mqttConfig.UserName);

        var clientOptions = _mqttClientOptionsFactory.CreateClientOptions(mqttConfig);

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
