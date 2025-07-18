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
        var serviceTarget = ServiceTarget.FromEntities(["light.kitchen", "light.livingroom"]);

        serviceTarget.EntityIds.Should().BeEquivalentTo("light.kitchen", "light.livingroom");
    }

    [Fact]
    public void ServiceTargetShouldContainCorrectEntitiesUsingParams()
    {
        var serviceTarget = ServiceTarget.FromEntities("light.kitchen", "light.livingroom");

        serviceTarget.EntityIds.Should().BeEquivalentTo("light.kitchen", "light.livingroom");
    }

    [Fact]
    public void ServiceTargetAndObjectShouldBeEqual()
    {
        var serviceTarget1 = ServiceTarget.FromEntity("light.kitchen");
        object serviceTarget2 = ServiceTarget.FromEntity("light.kitchen");
        serviceTarget1.Should().Be(serviceTarget2);
    }

    [Fact]
    public void ServiceTargetsShouldBeEqual()
    {
        var serviceTarget1 = ServiceTarget.FromEntity("light.kitchen");
        var serviceTarget2 = ServiceTarget.FromEntity("light.kitchen");
        serviceTarget1.Should().Be(serviceTarget2);
    }

    [Fact]
    public void ServiceTargetsOperatorEqualNotOverridden()
    {
        var serviceTarget1 = ServiceTarget.FromEntities("light.kitchen", "light.livingroom");
        var serviceTarget2 = ServiceTarget.FromEntities("light.kitchen", "light.livingroom");

        // The == operator is not overridden, so the == operator for ServiceTarget will only work on reference equality.
        Assert.False((serviceTarget1 == serviceTarget2), "ServiceTargets should not be equal using ==");
    }

    [Fact]
    public void ServiceTargetsOperatorEqualReference()
    {
        var serviceTarget1 = ServiceTarget.FromEntities("light.kitchen", "light.livingroom");
        var serviceTarget1b = serviceTarget1;   // Reference to the same object

        Assert.True((serviceTarget1 == serviceTarget1b), "ServiceTargets references should be equal using ==");
    }

    [Fact]
    public void ServiceTargetObjectEqualShouldBeTrue()
    {
        var serviceTarget1 = ServiceTarget.FromEntity("light.kitchen");
        object serviceTarget2 = ServiceTarget.FromEntity("light.kitchen");

        Assert.True(serviceTarget1.Equals(serviceTarget2), "ServiceTargets should be equal using Equals.object");
    }
}
