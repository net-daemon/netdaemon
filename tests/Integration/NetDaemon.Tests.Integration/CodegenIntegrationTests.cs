using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Extensions;
using NetDaemon.HassModel.CodeGenerator.Model;
using NetDaemon.Tests.Integration.Helpers;
using Xunit;

namespace NetDaemon.Tests.Integration;

public class CodegenIntegrationTests : IClassFixture<MakeSureNetDaemonIsRunningFixture>
{
    private readonly IHomeAssistantConnection _haConnection;

    public CodegenIntegrationTests(
        MakeSureNetDaemonIsRunningFixture _,
        IHomeAssistantConnection haConnection
    )
    {
        _haConnection = haConnection;
    }

    /// <summary>
    ///     Tests the code generator. We had som problems with websocket interface changing and this should at least allert us on changes when it reaches
    ///     beta stage
    /// </summary>
    [Fact]
    public async Task Codegen_ShouldBeAbleToParseServiceDescriptions()
    {
        var element = await _haConnection.GetServicesAsync(new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token).ConfigureAwait(false) ?? throw new InvalidOperationException("Failed to get services");
        var serviceMetadata = ServiceMetaDataParser.Parse(element);
        serviceMetadata.Count.Should().NotBe(0);

        var lightDomain = serviceMetadata.FirstOrDefault(n => n.Domain == "switch") ?? throw new InvalidOperationException("Expected domain light to be present");
        
        var turnOnService = lightDomain.Services.FirstOrDefault(n => n.Service == "turn_on") ?? throw new InvalidOperationException("Expected domain light to be present");
        
        Assert.NotNull(turnOnService?.Target?.Entity);
    }
}