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
        
    private Entity GetEntityWitState(string? state)
    {
        var haContextMock = new Mock<IHaContext>();

        var entityState = new EntityState { State = state };
            
        haContextMock.Setup(t => t.GetState("domain.testEntity")).Returns(entityState);
        return new Entity(haContextMock.Object, "domain.testEntity");
    }
}