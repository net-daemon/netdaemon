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
    public void ServiceTargetAndObjectShouldNotBeEqual()
    {
        var serviceTarget1 = ServiceTarget.FromEntity("light.kitchen");
        object serviceTarget2 = ServiceTarget.FromEntity("light.livingroom");
        serviceTarget1.Should().NotBe(serviceTarget2);
    }

    [Fact]
    public void ServiceTargetsShouldBeEqual()
    {
        var serviceTarget1 = ServiceTarget.FromEntity("light.kitchen");
        var serviceTarget2 = ServiceTarget.FromEntity("light.kitchen");
        serviceTarget1.Should().Be(serviceTarget2);
    }

    [Fact]
    public void ServiceTargetsShouldNotBeEqual()
    {
        var serviceTarget1 = ServiceTarget.FromEntity("light.kitchen");
        var serviceTarget2 = ServiceTarget.FromEntity("light.livingroom");
        serviceTarget1.Should().NotBe(serviceTarget2);
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
    public void ServiceTargetsOperatorNotEqualNorOverridden()
    {
        var serviceTarget1 = ServiceTarget.FromEntities("light.kitchen", "light.livingroom");
        var serviceTarget2 = ServiceTarget.FromEntities("light.kitchen", "light.kitchen");

        // The == operator is not overridden, so the == operator for ServiceTarget will only work on reference equality.  You will ALWAYS get true for not equal if not the same reference object
        Assert.True((serviceTarget1 != serviceTarget2), "ServiceTargets should not be equal using !=");
    }

    [Fact]
    public void ServiceTargetsOperatorEqualReference()
    {
        var serviceTarget1 = ServiceTarget.FromEntities("light.kitchen", "light.livingroom");
        var serviceTarget1b = serviceTarget1;   // Reference to the same object

        Assert.True((serviceTarget1 == serviceTarget1b), "ServiceTargets references should be equal using ==");
    }

    [Fact]
    public void ServiceTargetsOperatorNotEqualReference()
    {
        var serviceTarget1 = ServiceTarget.FromEntity("light.kitchen");
        var serviceTarget2 = ServiceTarget.FromEntity("light.livingroom");
        var serviceTarget1b = serviceTarget1;   // Reference to the same object
        var serviceTarget2b = serviceTarget2;

        Assert.True((serviceTarget1b != serviceTarget2b), "ServiceTargets references of different objects should not be equal using !=");
    }

    [Fact]
    public void ServiceTargetObjectEqualShouldBeTrue()
    {
        var serviceTarget1 = ServiceTarget.FromEntity("light.kitchen");
        object serviceTarget2 = ServiceTarget.FromEntity("light.kitchen");

        Assert.True(serviceTarget1.Equals(serviceTarget2), "ServiceTargets should be equal using Equals.object");
    }

    [Fact]
    public void ServiceTargetObjectEqualShouldNotBeTrue()
    {
        var serviceTarget1 = ServiceTarget.FromEntity("light.kitchen");
        object serviceTarget2 = ServiceTarget.FromEntity("light.livingroom");

        Assert.False(serviceTarget1.Equals(serviceTarget2), "ServiceTargets should not be equal using Equals.object");
    }

}
