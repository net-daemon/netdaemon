using MQTTnet;
using NetDaemon.Extensions.MqttEntityManager;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

public class MqttClientOptionsFactoryTests
{
    private MqttClientOptionsFactory MqttClientOptionsFactory { get; } = new();

    [Fact]
    public void CreatesDefaultConfiguration()
    {
        // This is the bare minimum necessary to establish a connection to an MQTT broker that doesn't use TLS
        // or require authentication. The default port is 1883 and a TCP connection is used.
        var mqttConfiguration = new MqttConfiguration
        {
            Host = "broker",
        };

        var mqttClientOptions = MqttClientOptionsFactory.CreateClientOptions(mqttConfiguration);

        mqttClientOptions.Should().NotBeNull();

        mqttClientOptions.ChannelOptions.Should().NotBeNull();
        mqttClientOptions.ChannelOptions.Should().BeOfType<MqttClientTcpOptions>();

        var mqttClientChannelOptions = (MqttClientTcpOptions)mqttClientOptions.ChannelOptions;

        var ipEndpoint = (System.Net.DnsEndPoint)mqttClientChannelOptions.RemoteEndpoint;
        ipEndpoint.Host.Should().Be("broker");
        ipEndpoint.Port.Should().Be(1883);

        mqttClientOptions.Credentials.Should().BeNull();

        mqttClientOptions.ChannelOptions.TlsOptions.UseTls.Should().BeFalse();
        mqttClientOptions.ChannelOptions.TlsOptions.AllowUntrustedCertificates.Should().BeFalse();
    }

    [Fact]
    public void CreatesDefaultConfigurationWithTls()
    {
        var mqttConfiguration = new MqttConfiguration
        {
            Host = "broker",
            UseTls = true
        };

        var mqttClientOptions = MqttClientOptionsFactory.CreateClientOptions(mqttConfiguration);

        mqttClientOptions.Should().NotBeNull();

        mqttClientOptions.ChannelOptions.Should().NotBeNull();
        mqttClientOptions.ChannelOptions.Should().BeOfType<MqttClientTcpOptions>();

        var mqttClientChannelOptions = (MqttClientTcpOptions)mqttClientOptions.ChannelOptions;

        var ipEndpoint = (System.Net.DnsEndPoint)mqttClientChannelOptions.RemoteEndpoint;
        ipEndpoint.Host.Should().Be("broker");
        ipEndpoint.Port.Should().Be(1883);

        mqttClientOptions.Credentials.Should().BeNull();

        mqttClientOptions.ChannelOptions.TlsOptions.UseTls.Should().BeTrue();

        // This would only get set to true if it and UseTls are both true
        mqttClientOptions.ChannelOptions.TlsOptions.AllowUntrustedCertificates.Should().BeFalse();
    }

    [Fact]
    public void IgnoresTlsCustomizationIfTlsIsntEnabled()
    {
        var mqttConfiguration = new MqttConfiguration
        {
            Host = "broker",
            UseTls = false,
            AllowUntrustedCertificates = true
        };

        var mqttClientOptions = MqttClientOptionsFactory.CreateClientOptions(mqttConfiguration);

        mqttClientOptions.Should().NotBeNull();

        mqttClientOptions.ChannelOptions.Should().NotBeNull();
        mqttClientOptions.ChannelOptions.Should().BeOfType<MqttClientTcpOptions>();

        var mqttClientChannelOptions = (MqttClientTcpOptions)mqttClientOptions.ChannelOptions;

        var ipEndpoint = (System.Net.DnsEndPoint)mqttClientChannelOptions.RemoteEndpoint;
        ipEndpoint.Host.Should().Be("broker");
        ipEndpoint.Port.Should().Be(1883);

        mqttClientOptions.Credentials.Should().BeNull();

        mqttClientOptions.ChannelOptions.TlsOptions.UseTls.Should().BeFalse();

        // This would only get set to true if it and UseTls are both true
        mqttClientOptions.ChannelOptions.TlsOptions.AllowUntrustedCertificates.Should().BeFalse();
    }

    [Fact]
    public void CreatesFullyCustomizedConfiguration()
    {
        var mqttConfiguration = new MqttConfiguration
        {
            Host = "broker",
            Port = 1234,
            UserName = "testuser",
            Password = "testpassword",
            UseTls = true,
            AllowUntrustedCertificates = true
        };

        var mqttClientOptions = MqttClientOptionsFactory.CreateClientOptions(mqttConfiguration);

        mqttClientOptions.Should().NotBeNull();

        mqttClientOptions.ChannelOptions.Should().NotBeNull();
        mqttClientOptions.ChannelOptions.Should().BeOfType<MqttClientTcpOptions>();

        var mqttClientChannelOptions = (MqttClientTcpOptions)mqttClientOptions.ChannelOptions;

        var ipEndpoint = (System.Net.DnsEndPoint)mqttClientChannelOptions.RemoteEndpoint;
        ipEndpoint.Host.Should().Be("broker");
        ipEndpoint.Port.Should().Be(1234);

        mqttClientOptions.Credentials.Should().NotBeNull();
        mqttClientOptions.Credentials.Should().BeOfType<MqttClientCredentials>();

        mqttClientOptions.Credentials.GetUserName(mqttClientOptions).Should().Be("testuser");
        mqttClientOptions.Credentials.GetPassword(mqttClientOptions).Should().BeEquivalentTo(Encoding.UTF8.GetBytes("testpassword"));

        mqttClientOptions.ChannelOptions.TlsOptions.UseTls.Should().BeTrue();
        mqttClientOptions.ChannelOptions.TlsOptions.AllowUntrustedCertificates.Should().BeTrue();
    }

    [Fact]
    void ThrowsArgumentNullExceptionIfMqttConfigIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => MqttClientOptionsFactory.CreateClientOptions(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    void ThrowsArgumentExceptionIfMqttConfigHasNullOrEmptyHost(string? host)
    {
        var mqttConfiguration = new MqttConfiguration
        {
            Host = host!,
        };

        Assert.Throws<ArgumentException>(() => MqttClientOptionsFactory.CreateClientOptions(mqttConfiguration));
    }
}
