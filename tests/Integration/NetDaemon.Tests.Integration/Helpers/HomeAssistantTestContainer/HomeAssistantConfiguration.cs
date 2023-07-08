using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

namespace NetDaemon.Tests.Integration.Helpers.HomeAssistantTestContainer;

public class HomeAssistantConfiguration : ContainerConfiguration
{
    public string Username { get; } = null!;
    public string Password { get; } = null!;
    public string ClientId { get; } = null!;
    public string Version { get; } = null!;

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

    public HomeAssistantConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
        : base(resourceConfiguration)
    {
    }

    public HomeAssistantConfiguration(IContainerConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
    }
    
    public HomeAssistantConfiguration(HomeAssistantConfiguration oldValue, HomeAssistantConfiguration newValue)
        : base(oldValue, newValue)
    {
        ClientId = BuildConfiguration.Combine(oldValue.ClientId, newValue.ClientId);
        Username = BuildConfiguration.Combine(oldValue.Username, newValue.Username);
        Password = BuildConfiguration.Combine(oldValue.Password, newValue.Password);
        Version = BuildConfiguration.Combine(oldValue.Version, newValue.Version);
    }
}