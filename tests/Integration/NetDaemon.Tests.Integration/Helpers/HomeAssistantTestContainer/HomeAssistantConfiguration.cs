using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

namespace NetDaemon.Tests.Integration.Helpers.HomeAssistantTestContainer;

/// <summary>
/// Container configuration for Home Assistant integration tests.
/// </summary>
public class HomeAssistantConfiguration : ContainerConfiguration
{
    /// <summary>
    /// Gets the Home Assistant onboarding username.
    /// </summary>
    public string Username { get; } = null!;

    /// <summary>
    /// Gets the Home Assistant onboarding password.
    /// </summary>
    public string Password { get; } = null!;

    /// <summary>
    /// Gets the Home Assistant OAuth client id.
    /// </summary>
    public string ClientId { get; } = null!;

    /// <summary>
    /// Gets the Home Assistant container image version.
    /// </summary>
    public string Version { get; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeAssistantConfiguration"/> class.
    /// </summary>
    /// <param name="username">The Home Assistant onboarding username.</param>
    /// <param name="password">The Home Assistant onboarding password.</param>
    /// <param name="clientId">The Home Assistant OAuth client id.</param>
    /// <param name="version">The Home Assistant container image version.</param>
    public HomeAssistantConfiguration(
        string? username = null,
        string? password = null,
        string? clientId = null,
        string? version = null)
    {
        Username = username!;
        Password = password!;
        ClientId = clientId!;
        Version = version!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeAssistantConfiguration"/> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public HomeAssistantConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
        : base(resourceConfiguration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeAssistantConfiguration"/> class.
    /// </summary>
    /// <param name="resourceConfiguration">The container configuration.</param>
    public HomeAssistantConfiguration(IContainerConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeAssistantConfiguration"/> class.
    /// </summary>
    /// <param name="oldValue">The previous Home Assistant configuration.</param>
    /// <param name="newValue">The new Home Assistant configuration.</param>
    public HomeAssistantConfiguration(HomeAssistantConfiguration oldValue, HomeAssistantConfiguration newValue)
        : base(oldValue, newValue)
    {
        ClientId = BuildConfiguration.Combine(oldValue.ClientId, newValue.ClientId);
        Username = BuildConfiguration.Combine(oldValue.Username, newValue.Username);
        Password = BuildConfiguration.Combine(oldValue.Password, newValue.Password);
        Version = BuildConfiguration.Combine(oldValue.Version, newValue.Version);
    }
}
