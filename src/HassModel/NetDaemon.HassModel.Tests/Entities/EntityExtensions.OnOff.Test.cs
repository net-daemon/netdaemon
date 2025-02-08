using NetDaemon.HassModel.Entities;

namespace NetDaemon.HassModel.Tests.Entities;

public class EntityExtensionsOnOffTest
{
    [Theory]
    [InlineData("on", true)]
    [InlineData("ON", true)]
    [InlineData("off", false)]
    [InlineData("blabla", false)]
    [InlineData("unavaliable", false)]
    [InlineData(null, false)]
    public void IsOn(string? state, bool isOn)
    {
        GetEntityWithState(state).IsOn().Should().Be(isOn);
        GetEntityWithState(state).EntityState.IsOn().Should().Be(isOn);
    }

    [Theory]
    [InlineData("off", true)]
    [InlineData("OFF", true)]
    [InlineData("on", false)]
    [InlineData("blabla", false)]
    [InlineData("unavaliable", false)]
    [InlineData(null, false)]
    public void IsOff(string? state, bool isOn)
    {
        GetEntityWithState(state).IsOff().Should().Be(isOn);
        GetEntityWithState(state).EntityState.IsOff().Should().Be(isOn);
    }

    private static Entity GetEntityWithState(string? state)
    {
        var haContextMock = new Mock<IHaContext>();

        var entityState = new EntityState { State = state };

        haContextMock.Setup(t => t.GetState("domain.testEntity")).Returns(entityState);
        return new Entity(haContextMock.Object, "domain.testEntity");
    }
}
