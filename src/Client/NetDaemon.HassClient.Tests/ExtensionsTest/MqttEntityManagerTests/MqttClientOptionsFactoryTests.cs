using MQTTnet.Client;
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

        mqttClientOptions.ClientOptions.ChannelOptions.Should().NotBeNull();
        mqttClientOptions.ClientOptions.ChannelOptions.Should().BeOfType<MqttClientTcpOptions>();

        var mqttClientChannelOptions = (MqttClientTcpOptions)mqttClientOptions.ClientOptions.ChannelOptions;
        mqttClientChannelOptions.Server.Should().Be("broker");
        mqttClientChannelOptions.Port.Should().Be(1883);

        mqttClientOptions.ClientOptions.Credentials.Should().BeNull();

        mqttClientOptions.ClientOptions.ChannelOptions.TlsOptions.UseTls.Should().BeFalse();
        mqttClientOptions.ClientOptions.ChannelOptions.TlsOptions.AllowUntrustedCertificates.Should().BeFalse();
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

        mqttClientOptions.ClientOptions.ChannelOptions.Should().NotBeNull();
        mqttClientOptions.ClientOptions.ChannelOptions.Should().BeOfType<MqttClientTcpOptions>();

        var mqttClientChannelOptions = (MqttClientTcpOptions)mqttClientOptions.ClientOptions.ChannelOptions;
        mqttClientChannelOptions.Server.Should().Be("broker");
        mqttClientChannelOptions.Port.Should().Be(1883);

        mqttClientOptions.ClientOptions.Credentials.Should().BeNull();

        mqttClientOptions.ClientOptions.ChannelOptions.TlsOptions.UseTls.Should().BeTrue();

        // This would only get set to true if it and UseTls are both true
        mqttClientOptions.ClientOptions.ChannelOptions.TlsOptions.AllowUntrustedCertificates.Should().BeFalse();
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

        mqttClientOptions.ClientOptions.ChannelOptions.Should().NotBeNull();
        mqttClientOptions.ClientOptions.ChannelOptions.Should().BeOfType<MqttClientTcpOptions>();

        var mqttClientChannelOptions = (MqttClientTcpOptions)mqttClientOptions.ClientOptions.ChannelOptions;
        mqttClientChannelOptions.Server.Should().Be("broker");
        mqttClientChannelOptions.Port.Should().Be(1883);

        mqttClientOptions.ClientOptions.Credentials.Should().BeNull();

        mqttClientOptions.ClientOptions.ChannelOptions.TlsOptions.UseTls.Should().BeFalse();

        // This would only get set to true if it and UseTls are both true
        mqttClientOptions.ClientOptions.ChannelOptions.TlsOptions.AllowUntrustedCertificates.Should().BeFalse();
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

        mqttClientOptions.ClientOptions.ChannelOptions.Should().NotBeNull();
        mqttClientOptions.ClientOptions.ChannelOptions.Should().BeOfType<MqttClientTcpOptions>();

        var mqttClientChannelOptions = (MqttClientTcpOptions)mqttClientOptions.ClientOptions.ChannelOptions;
        mqttClientChannelOptions.Server.Should().Be("broker");
        mqttClientChannelOptions.Port.Should().Be(1234);

        mqttClientOptions.ClientOptions.Credentials.Should().NotBeNull();
        mqttClientOptions.ClientOptions.Credentials.Should().BeOfType<MqttClientCredentials>();

        mqttClientOptions.ClientOptions.Credentials.GetUserName(mqttClientOptions.ClientOptions).Should().Be("testuser");
        mqttClientOptions.ClientOptions.Credentials.GetPassword(mqttClientOptions.ClientOptions).Should().BeEquivalentTo(Encoding.UTF8.GetBytes("testpassword"));

        mqttClientOptions.ClientOptions.ChannelOptions.TlsOptions.UseTls.Should().BeTrue();
        mqttClientOptions.ClientOptions.ChannelOptions.TlsOptions.AllowUntrustedCertificates.Should().BeTrue();
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
