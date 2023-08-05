using NetDaemon.Client.Settings;
using NetDaemon.HassModel.CodeGenerator;

namespace NetDaemon.HassModel.Tests.CodeGenerator;

public class ControllerTest
{
    [Fact]
    public async Task ControllerShouldReturnDefaultValueForMetadata()
    {
        // ARRANGE
        var controller = new Controller(new CodeGenerationSettings(), new HomeAssistantSettings());

        // ACT
        var result = await controller.LoadEntitiesMetaDataAsync();

        // ASSERT
        result.Domains.Count.Should().Be(2);
        result.Domains.Single(x => x.Domain == "light").Should().NotBeNull();
        result.Domains.Single(x => x.Domain == "light").Attributes.SingleOrDefault(x => x.JsonName == "brightness").Should().NotBeNull();
        result.Domains.Single(x => x.Domain == "media_player").Should().NotBeNull();
        result.Domains.Single(x => x.Domain == "media_player").Attributes.SingleOrDefault(x => x.JsonName == "media_artist").Should().NotBeNull();
    }
}