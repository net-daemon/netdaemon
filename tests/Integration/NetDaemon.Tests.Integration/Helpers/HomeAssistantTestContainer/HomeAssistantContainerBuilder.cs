using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using Microsoft.Extensions.Logging;

namespace NetDaemon.Tests.Integration.Helpers.HomeAssistantTestContainer;

/// <summary>
/// Builds Home Assistant containers for integration tests.
/// </summary>
public class HomeAssistantContainerBuilder : ContainerBuilder<HomeAssistantContainerBuilder, HomeAssistantContainer, HomeAssistantConfiguration>
{
    /// <summary>
    /// The default Home Assistant container image version.
    /// </summary>
    public const string DefaultVersion = "stable";

    /// <summary>
    /// The default OAuth client id used during Home Assistant onboarding.
    /// </summary>
    public const string DefaultClientId = "http://dummyClientId";

    /// <summary>
    /// The default Home Assistant onboarding username.
    /// </summary>
    public const string DefaultUsername = "username";

    /// <summary>
    /// The default Home Assistant onboarding password.
    /// </summary>
    public const string DefaultPassword = "password";

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeAssistantContainerBuilder"/> class.
    /// </summary>
    public HomeAssistantContainerBuilder() : this(new HomeAssistantConfiguration())
    {
        DockerResourceConfiguration = Init().DockerResourceConfiguration;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeAssistantContainerBuilder"/> class.
    /// </summary>
    /// <param name="dockerResourceConfiguration">The Home Assistant container configuration.</param>
    public HomeAssistantContainerBuilder(HomeAssistantConfiguration dockerResourceConfiguration) : base(dockerResourceConfiguration)
    {
        DockerResourceConfiguration = dockerResourceConfiguration;
    }

    protected override HomeAssistantContainerBuilder Init() =>
        base.Init()
            .WithImage($"homeassistant/home-assistant:{DefaultVersion}")
            .WithPortBinding(8123, true)
            .WithWaitStrategy(
                Wait.ForUnixContainer().
                UntilHttpRequestIsSucceeded(request => request.ForPort(8123).ForPath("/")))
            .WithUsername(DefaultUsername)
            .WithPassword(DefaultPassword)
            .WithClientId(DefaultClientId)
            .WithVersion(DefaultVersion);

    /// <summary>
    /// Sets the Home Assistant container image version.
    /// </summary>
    /// <param name="version">The Home Assistant container image version.</param>
    /// <returns>The configured builder.</returns>
    public HomeAssistantContainerBuilder WithVersion(string version) =>
        Merge(DockerResourceConfiguration, new HomeAssistantConfiguration(version: version))
            .WithImage($"homeassistant/home-assistant:{version}");

    /// <summary>
    /// Sets the Home Assistant onboarding username.
    /// </summary>
    /// <param name="username">The Home Assistant onboarding username.</param>
    /// <returns>The configured builder.</returns>
    public HomeAssistantContainerBuilder WithUsername(string username) => Merge(DockerResourceConfiguration, new HomeAssistantConfiguration(username: username));

    /// <summary>
    /// Sets the Home Assistant onboarding password.
    /// </summary>
    /// <param name="password">The Home Assistant onboarding password.</param>
    /// <returns>The configured builder.</returns>
    public HomeAssistantContainerBuilder WithPassword(string password) => Merge(DockerResourceConfiguration, new HomeAssistantConfiguration(password: password));

    /// <summary>
    /// Sets the Home Assistant OAuth client id.
    /// </summary>
    /// <param name="clientId">The Home Assistant OAuth client id.</param>
    /// <returns>The configured builder.</returns>
    public HomeAssistantContainerBuilder WithClientId(string clientId) => Merge(DockerResourceConfiguration, new HomeAssistantConfiguration(clientId: clientId));

    /// <inheritdoc />
    public override HomeAssistantContainer Build()
    {
        Validate();
        return new HomeAssistantContainer(DockerResourceConfiguration );
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
