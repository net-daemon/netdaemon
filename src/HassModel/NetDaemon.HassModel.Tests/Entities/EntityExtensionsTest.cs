using System.Text.Json;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.HassModel.Tests.Entities;

public class EntityExtensionsTest
{
    [Theory]
    [InlineData("on", true)]
    [InlineData("ON", true)]
    [InlineData("off", false)]
    [InlineData("blabla", false)]
    [InlineData("unavaliable", false)]
    [InlineData(null, false)]
    public void IsOntest(string? state, bool isOn)
    {
        GetEntityWitState(state).IsOn().Should().Be(isOn);
        GetEntityWitState(state).EntityState.IsOn().Should().Be(isOn);
    }

    [Theory]
    [InlineData("off", true)]
    [InlineData("OFF", true)]
    [InlineData("on", false)]
    [InlineData("blabla", false)]
    [InlineData("unavaliable", false)]
    [InlineData(null, false)]
    public void IsOftest(string? state, bool isOn)
    {
        GetEntityWitState(state).IsOff().Should().Be(isOn);
        GetEntityWitState(state).EntityState.IsOff().Should().Be(isOn);
    }

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

    private static Entity GetEntityWitState(string? state)
    {
        var haContextMock = new Mock<IHaContext>();

        var entityState = new EntityState { State = state };

        haContextMock.Setup(t => t.GetState("domain.testEntity")).Returns(entityState);
        return new Entity(haContextMock.Object, "domain.testEntity");
    }
}
