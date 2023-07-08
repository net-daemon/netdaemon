using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

namespace NetDaemon.Tests.Integration.Helpers.HomeAssistantTestContainer;

public class HomeAssistantContainerBuilder : ContainerBuilder<HomeAssistantContainerBuilder, HomeAssistantContainer, HomeAssistantConfiguration>
{
    public const string DefaultVersion = "stable";
    public const string DefaultClientId = "http://dummyClientId";
    public const string DefaultUsername = "username";
    public const string DefaultPassword = "password";
    
    public HomeAssistantContainerBuilder() : this(new HomeAssistantConfiguration())
    {
        DockerResourceConfiguration = Init().DockerResourceConfiguration;
    }

    public HomeAssistantContainerBuilder(HomeAssistantConfiguration dockerResourceConfiguration) : base(dockerResourceConfiguration)
    {
        DockerResourceConfiguration = dockerResourceConfiguration;
    }

    protected override HomeAssistantContainerBuilder Init() =>
        base.Init()
            .WithImage($"homeassistant/home-assistant:{DefaultVersion}")
            .WithPortBinding(8123, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(request => request.ForPort(8123).ForPath("/")))
            .WithUsername(DefaultUsername)
            .WithPassword(DefaultPassword)
            .WithClientId(DefaultClientId)
            .WithVersion(DefaultVersion);

    public HomeAssistantContainerBuilder WithVersion(string version) =>
        Merge(DockerResourceConfiguration, new HomeAssistantConfiguration(version: version))
            .WithImage($"homeassistant/home-assistant:{version}");

    public HomeAssistantContainerBuilder WithUsername(string username) => Merge(DockerResourceConfiguration, new HomeAssistantConfiguration(username: username));
    public HomeAssistantContainerBuilder WithPassword(string password) => Merge(DockerResourceConfiguration, new HomeAssistantConfiguration(password: password));
    public HomeAssistantContainerBuilder WithClientId(string clientId) => Merge(DockerResourceConfiguration, new HomeAssistantConfiguration(clientId: clientId));

    public override HomeAssistantContainer Build()
    {
        Validate();
        return new HomeAssistantContainer(DockerResourceConfiguration, TestcontainersSettings.Logger);
    }

    protected override HomeAssistantContainerBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new HomeAssistantConfiguration(resourceConfiguration));
    }

    protected override HomeAssistantContainerBuilder Merge(HomeAssistantConfiguration oldValue, HomeAssistantConfiguration newValue)
    {
        return new HomeAssistantContainerBuilder(new HomeAssistantConfiguration(oldValue, newValue));
    }

    protected override HomeAssistantConfiguration DockerResourceConfiguration { get; }

    protected override HomeAssistantContainerBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new HomeAssistantConfiguration(resourceConfiguration));
    }
}