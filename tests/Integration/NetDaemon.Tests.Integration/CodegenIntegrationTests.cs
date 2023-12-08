using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Extensions;
using NetDaemon.HassModel.CodeGenerator.Model;
using NetDaemon.Tests.Integration.Helpers;
using Xunit;

namespace NetDaemon.Tests.Integration;

public class CodegenIntegrationTests : NetDaemonIntegrationBase
{
    /// <summary>
    ///     Tests the code generator. We had som problems with websocket interface changing and this should at least allert us on changes when it reaches
    ///     beta stage
    /// </summary>
    [Fact]
    public async Task Codegen_ShouldBeAbleToParseServiceDescriptions()
    {
        var haConnection = Services.GetRequiredService<IHomeAssistantConnection>();

        var element = await haConnection.GetServicesAsync(new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token).ConfigureAwait(false) ?? throw new InvalidOperationException("Failed to get services");
        var serviceMetadata = ServiceMetaDataParser.Parse(element, out var errors);

        errors.Should().BeEmpty();
        serviceMetadata.Count.Should().NotBe(0);

        var lightDomain = serviceMetadata.FirstOrDefault(n => n.Domain == "switch") ?? throw new InvalidOperationException("Expected domain light to be present");

        var turnOnService = lightDomain.Services.FirstOrDefault(n => n.Service == "turn_on") ?? throw new InvalidOperationException("Expected domain light to be present");

        Assert.NotNull(turnOnService.Target?.Entity);
    }

    public CodegenIntegrationTests(HomeAssistantLifetime homeAssistantLifetime) : base(homeAssistantLifetime)
    {
    }
}
