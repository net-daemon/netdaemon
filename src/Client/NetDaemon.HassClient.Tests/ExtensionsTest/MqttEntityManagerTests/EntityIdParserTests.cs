using System.Collections;
using NetDaemon.Extensions.MqttEntityManager.Helpers;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

public class EntityIdParserTests
{
    sealed class GoodEntityIdTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return ["domain.id", new ValueTuple<string, string>("domain", "id")];
            yield return ["domain.id.with.dots", new ValueTuple<string, string>("domain", "id.with.dots")];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    sealed class BadEntityIdTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return ["no-dots"];
            yield return ["domain."];
            yield return [".identifier"];
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
            .WithMessage("*entityId*");
    }

    [Fact]
    public void ThrowsOnEmpty()
    {
        Action act = () => EntityIdParser.Extract("  ");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*entityId*");
    }
}
