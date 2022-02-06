using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using NetDaemon.Extensions.MqttEntityManager.Exceptions;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Wrapper to assure an MQTT connection
/// </summary>
internal class AssuredMqttConnection : IAssuredMqttConnection, IDisposable
{
    private const int RetryMaximumSeconds = 15;
    private readonly ILogger<AssuredMqttConnection> _logger;
    private readonly Task? _connectionTask;
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
        IMqttFactory mqttFactory,
        IOptions<MqttConfiguration> mqttConfig)
    {
        _logger = logger;

        _connectionTask = Task.Run(() => ConnectAsync(mqttConfig.Value, mqttFactory));
    }

    /// <summary>
    /// Ensures that an MQTT client is available, retrying if necessary, and throws if connection is impossible
    /// </summary>
    /// <returns></returns>
    /// <exception cref="MqttConnectionException">Timed out while waiting for connection</exception>
    public IManagedMqttClient GetClientOrThrow()
    {
        _connectionTask?.Wait();

        var retryCount = 1;
        while ((_mqttClient == null || !_mqttClient.IsConnected) && retryCount <= RetryMaximumSeconds)
        {
            _logger.LogDebug("MQTT client is not yet available, retry #{RetryCount}", retryCount);
            Thread.Sleep(1000);
            retryCount++;
        }

        if (_mqttClient == null || !_mqttClient.IsConnected)
            throw new MqttConnectionException("MQTT client is not connected");

        return _mqttClient;
    }

    private async Task ConnectAsync(MqttConfiguration mqttConfig, IMqttFactory mqttFactory)
    {
        _logger.LogDebug("Connecting to MQTT broker at {Host}:{Port}/{UserName}", 
            mqttConfig.Host, mqttConfig.Port, mqttConfig.UserName);

        var clientOptions = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithTcpServer(mqttConfig.Host, mqttConfig.Port)
                .WithCredentials(mqttConfig.UserName, mqttConfig.Password))
            .Build();

        _mqttClient = mqttFactory.CreateManagedMqttClient();

        _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(LogConnectedStatus);
        _mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(LogDisconnectedStatus);

        await _mqttClient.StartAsync(clientOptions);
    }

    private void LogDisconnectedStatus(MqttClientDisconnectedEventArgs arg)
    {
        _logger.LogDebug("MQTT disconnected: {Reason}", arg.Reason);
    }

    private void LogConnectedStatus(MqttClientConnectedEventArgs arg)
    {
        _logger.LogDebug("MQTT connected: {ResultCode}", arg.ConnectResult.ResultCode);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _logger.LogInformation("MQTT disconnecting");
        _connectionTask?.Dispose();
        _mqttClient?.Dispose();
    }
}