using System.Collections;
using NetDaemon.Extensions.MqttEntityManager.Helpers;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

public class EntityIdParserTests
{
    class GoodEntityIdTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "domain.id", new ValueTuple<string, string>("domain", "id") };
            yield return new object[] { "domain.id.with.dots", new ValueTuple<string, string>("domain", "id.with.dots") };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    class BadEntityIdTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "no-dots"};
            yield return new object[] { "domain."};
            yield return new object[] { ".identifier"};
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    [Theory]
    [ClassData(typeof(GoodEntityIdTestData))]
    public void CanExtract(string entityId, (string domain, string identifier) expected)
    {
        var response = EntityIdParser.Extract(entityId);

        response.Should().Be(expected);
    }
    
    [Theory]
    [ClassData(typeof(BadEntityIdTestData))]
    public void FailsOnBadData(string entityId)
    {
        Action act = () => EntityIdParser.Extract(entityId);

        act.Should().Throw<ArgumentException>()
            .Where(e => e.Message.Contains("should be of the format"));
    }

    [Fact]
    public void ThrowsOnNull()
    {
        Action act = () => EntityIdParser.Extract(null!);

        act.Should().Throw<ArgumentException>()
            .WithMessage("entityId");
    }
    
    [Fact]
    public void ThrowsOnEmpty()
    {
        Action act = () => EntityIdParser.Extract("  ");

        act.Should().Throw<ArgumentException>()
            .WithMessage("entityId");
    }
}