using System.Text.Json;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.HassModel.Tests.Entities;

public class EntityExtensionsCallServiceWithResponseAsyncTest
{
    [Fact]
    public async Task CallServiceWithResponseAsyncShouldReturnCorrectData()
    {
        var haContextMock = new Mock<IHaContext>();
        var entity = new Entity(haContextMock.Object, "domain.test_entity");

        var response = JsonDocument.Parse("{\"test\": \"test\"}").RootElement;

        haContextMock.Setup(t => t.CallServiceWithResponseAsync("domain", "test_service", It.IsAny<ServiceTarget>(), It.IsAny<object?>()))
            .Returns(Task.FromResult((JsonElement?) response));

        var result = await entity.CallServiceWithResponseAsync("test_service", new { test = "test" });
        result.Should().NotBeNull();
        result!.Value.GetProperty("test").GetString().Should().Be("test");
    }
}
