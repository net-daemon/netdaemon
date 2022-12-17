using NetDaemon.HassModel.Entities;

namespace NetDaemon.HassModel.Tests;

public class ServiceTargetTest
{
    [Fact]
    public void ServiceTargetShouldContainCorrectEntity()
    {
        var serviceTarget = ServiceTarget.FromEntity("light.kitchen");

        serviceTarget.EntityIds.Should().BeEquivalentTo("light.kitchen");
    }

    [Fact]
    public void ServiceTargetShouldContainCorrectEntities()
    {
        var serviceTarget = ServiceTarget.FromEntities(new[] { "light.kitchen", "light.livingroom" });

        serviceTarget.EntityIds.Should().BeEquivalentTo("light.kitchen", "light.livingroom");
    }

    [Fact]
    public void ServiceTargetShouldContainCorrectEntitiesUsingParams()
    {
        var serviceTarget = ServiceTarget.FromEntities("light.kitchen", "light.livingroom");

        serviceTarget.EntityIds.Should().BeEquivalentTo("light.kitchen", "light.livingroom");
    }
}