using System.Collections;
using System.Text.Json.Nodes;
using NetDaemon.Extensions.MqttEntityManager.Helpers;
namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

public class JsonNodeExtensionTests
{
    class MergeTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { J(new {a = "1", b = "2"}), J(new {}), J( new {a = "1", b ="2"})};
            yield return new object[] { J(new {a = "1", b = "2"}), J(new {c = "3"}), J( new {a = "1", b ="2", c = "3"})};
            yield return new object[] { J(new {a = "1", b = "2"}), J(new {a = "5"}), J( new {a = "5", b ="2"})};
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    [Theory]
    [ClassData(typeof(MergeTestData))]
    public void CanMerge(JsonObject target, JsonObject toMerge, JsonObject expected)
    {
        target.AddRange(toMerge);

        // Getting cyclic errors comparing the json structs so setup simple dictionaries to compare
        var targetDict = JsonSerializer.Deserialize<Dictionary<string, string>>(target.ToJsonString());
        var expectedDict = JsonSerializer.Deserialize<Dictionary<string, string>>(expected.ToJsonString());

        targetDict.Should().BeEquivalentTo(expectedDict);
    }

    private static JsonObject J(dynamic o)
    {
        return JsonSerializer.SerializeToNode(o);
    }
}