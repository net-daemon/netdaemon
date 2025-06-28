using System.Globalization;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using NetDaemon.Extensions.MqttEntityManager.Exceptions;
using NetDaemon.Extensions.MqttEntityManager.Helpers;

namespace NetDaemon.Extensions.MqttEntityManager;

'''/// <summary>
/// Wrapper to assure an MQTT connection
/// </summary>
internal class AssuredMqttConnection : IAssuredMqttConnection, IDisposable
{
    private readonly ILogger<AssuredMqttConnection> _logger;
    private readonly IMqttClientOptionsFactory _mqttClientOptionsFactory;
    private readonly IMqttFactoryWrapper _mqttFactory;
    private readonly MqttConfiguration _mqttConfig;
    private readonly TaskCompletionSource<IMqttClient> _connectionTcs = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private IMqttClient? _mqttClient;
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
        _mqttFactory = mqttFactory;
        _mqttConfig = mqttConfig.Value;

        _logger.LogTrace("MQTT initiating connection");
        _ = Task.Run(() => ConnectAsync(_cancellationTokenSource.Token));
    }

    /// <summary>
    /// Ensures that the MQTT client is available
    /// </summary>
    /// <returns></returns>
    /// <exception cref="MqttConnectionException">Timed out while waiting for connection</exception>
    public async Task<IMqttClient> GetClientAsync()
    {
        return await _connectionTcs.Task.ConfigureAwait(false);
    }

    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Connecting to MQTT broker at {Host}:{Port}/{UserName}",
            _mqttConfig.Host, _mqttConfig.Port, _mqttConfig.UserName);

        var clientOptions = _mqttClientOptionsFactory.CreateClientOptions(_mqttConfig);

        _mqttClient = _mqttFactory.CreateMqttClient();

        _mqttClient.ConnectedAsync += MqttClientOnConnectedAsync;
        _mqttClient.DisconnectedAsync += MqttClientOnDisconnectedAsync;
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _mqttClient.ConnectAsync(clientOptions, cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to connect to MQTT broker, will retry in 5 seconds");
                await Task.Delay(5000, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private Task MqttClientOnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        _logger.LogDebug("MQTT disconnected: {Reason}", BuildErrorResponse(arg));
        if (_disposed) return Task.CompletedTask;
        
        _ = Task.Run(async () =>
        {
            await Task.Delay(5000).ConfigureAwait(false);
            await ConnectAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
        });
        return Task.CompletedTask;
    }

    private Task MqttClientOnConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        _logger.LogDebug("MQTT connected: {ResultCode}", arg.ConnectResult.ResultCode);
        _connectionTcs.TrySetResult(_mqttClient!);
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
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        if (_mqttClient is not null)
        {
            _mqttClient.ConnectedAsync -= MqttClientOnConnectedAsync;
            _mqttClient.DisconnectedAsync -= MqttClientOnDisconnectedAsync;
            if (_mqttClient.IsConnected)
                _mqttClient.DisconnectAsync().GetAwaiter().GetResult();
            _mqttClient.Dispose();
        }
    }
}''
