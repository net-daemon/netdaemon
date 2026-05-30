using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Packets;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Wrapper to assure an MQTT connection.
/// </summary>
internal class AssuredMqttConnection : IAssuredMqttConnection, IDisposable
{
    private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ConnectedPollDelay = TimeSpan.FromSeconds(1);

    private readonly ILogger<AssuredMqttConnection> _logger;
    private readonly IMqttClient _mqttClient;
    private readonly MqttClientOptions _clientOptions;
    private readonly Channel<MqttApplicationMessage> _publishQueue = Channel.CreateUnbounded<MqttApplicationMessage>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    private readonly ConcurrentDictionary<string, MqttTopicFilter> _subscriptions = new();
    private readonly CancellationTokenSource _stopping = new();
    private readonly SemaphoreSlim _connectionSignal = new(0, 1);
    private readonly Task _connectionTask;
    private readonly Task _publishTask;
    private bool _disposed;
    private volatile bool _hasConnected;

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
        IMqttFactory mqttFactory,
        IOptions<MqttConfiguration> mqttConfig)
    {
        _logger = logger;

        _logger.LogTrace("MQTT initiating connection");
        _clientOptions = mqttClientOptionsFactory.CreateClientOptions(mqttConfig.Value);
        _mqttClient = mqttFactory.CreateMqttClient();

        _mqttClient.ConnectedAsync += MqttClientOnConnectedAsync;
        _mqttClient.DisconnectedAsync += MqttClientOnDisconnectedAsync;
        _mqttClient.ApplicationMessageReceivedAsync += MqttClientOnApplicationMessageReceivedAsync;

        _connectionTask = Task.Run(() => MaintainConnectionAsync(_stopping.Token));
        _publishTask = Task.Run(() => PublishQueuedMessagesAsync(_stopping.Token));
    }

    /// <inheritdoc />
    public event Func<MqttApplicationMessageReceivedEventArgs, Task>? ApplicationMessageReceivedAsync;

    /// <inheritdoc />
    public async Task PublishAsync(MqttApplicationMessage message)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _publishQueue.Writer.WriteAsync(message, _stopping.Token).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SubscribeAsync(MqttTopicFilter topicFilter)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (string.IsNullOrEmpty(topicFilter.Topic))
        {
            throw new ArgumentException("MQTT topic filter must specify a topic.", nameof(topicFilter));
        }

        _subscriptions[topicFilter.Topic] = topicFilter;

        if (_mqttClient.IsConnected)
        {
            await SubscribeOnClientAsync(topicFilter, _stopping.Token).ConfigureAwait(false);
        }
    }

    private async Task MaintainConnectionAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_mqttClient.IsConnected)
                {
                    await Task.Delay(ConnectedPollDelay, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                _logger.LogTrace("Connecting to MQTT broker at {Host}:{Port}/{UserName}",
                    GetConfiguredHost(), GetConfiguredPort(), _clientOptions.Credentials?.GetUserName(_clientOptions));

                var connectResult = await _mqttClient.ConnectAsync(_clientOptions, cancellationToken).ConfigureAwait(false);

                if (connectResult.ResultCode != MqttClientConnectResultCode.Success)
                {
                    _logger.LogTrace("MQTT connection rejected: {ResultCode} {ReasonString}",
                        connectResult.ResultCode, connectResult.ReasonString);
                    await Task.Delay(ReconnectDelay, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                _logger.LogTrace("MQTT client is ready");
                _hasConnected = true;
                SignalConnection();
                await ResubscribeAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogTrace(ex, "MQTT connection attempt failed");
                await DelayReconnectAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task PublishQueuedMessagesAsync(CancellationToken cancellationToken)
    {
        await foreach (var message in _publishQueue.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!_mqttClient.IsConnected)
                    {
                        await WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    await _mqttClient.PublishAsync(message, cancellationToken).ConfigureAwait(false);
                    break;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to publish MQTT message. The message will be retried after reconnect.");
                    await WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }

    private async Task ResubscribeAsync(CancellationToken cancellationToken)
    {
        foreach (var topicFilter in _subscriptions.Values)
        {
            await SubscribeOnClientAsync(topicFilter, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task SubscribeOnClientAsync(MqttTopicFilter topicFilter, CancellationToken cancellationToken)
    {
        var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(topicFilter)
            .Build();

        await _mqttClient.SubscribeAsync(subscribeOptions, cancellationToken).ConfigureAwait(false);
    }

    private Task MqttClientOnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        if (_disposed || _stopping.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        if (_hasConnected)
        {
            _logger.LogDebug("MQTT disconnected: {Reason}", BuildErrorResponse(arg));
        }
        else
        {
            _logger.LogTrace("MQTT disconnected before the initial connection completed: {Reason}", BuildErrorResponse(arg));
        }

        return Task.CompletedTask;
    }

    private Task MqttClientOnConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        _hasConnected = true;
        _logger.LogDebug("MQTT connected: {ResultCode}", arg.ConnectResult.ResultCode);
        SignalConnection();
        return Task.CompletedTask;
    }

    private async Task MqttClientOnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        var handlers = ApplicationMessageReceivedAsync;
        if (handlers is null)
        {
            return;
        }

        foreach (Func<MqttApplicationMessageReceivedEventArgs, Task> handler in handlers.GetInvocationList())
        {
            await handler(arg).ConfigureAwait(false);
        }
    }

    private static string BuildErrorResponse(MqttClientDisconnectedEventArgs arg)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine(CultureInfo.InvariantCulture, $"{arg.Exception?.Message} ({arg.Reason})");
        var ex = arg.Exception?.InnerException;
        while (ex != null)
        {
            sb.AppendLine(ex.Message);
            ex = ex.InnerException;
        }

        return sb.ToString();
    }

    private void SignalConnection()
    {
        if (_connectionSignal.CurrentCount == 0)
        {
            _connectionSignal.Release();
        }
    }

    private async Task WaitForConnectionAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient.IsConnected)
        {
            return;
        }

        await _connectionSignal.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task DelayReconnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(ReconnectDelay, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private string? GetConfiguredHost()
    {
        return (_clientOptions.ChannelOptions as MqttClientTcpOptions)?.RemoteEndpoint switch
        {
            System.Net.DnsEndPoint dnsEndPoint => dnsEndPoint.Host,
            System.Net.IPEndPoint ipEndPoint => ipEndPoint.Address.ToString(),
            _ => null
        };
    }

    private int? GetConfiguredPort()
    {
        return (_clientOptions.ChannelOptions as MqttClientTcpOptions)?.RemoteEndpoint switch
        {
            System.Net.DnsEndPoint dnsEndPoint => dnsEndPoint.Port,
            System.Net.IPEndPoint ipEndPoint => ipEndPoint.Port,
            _ => null
        };
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _logger.LogTrace("MQTT disconnecting");
        _stopping.Cancel();
        _publishQueue.Writer.TryComplete();

        try
        {
            Task.WaitAll([_connectionTask, _publishTask], TimeSpan.FromSeconds(1));
        }
        catch (AggregateException)
        {
        }

        _stopping.Dispose();
        _connectionSignal.Dispose();

        if (_mqttClient is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
