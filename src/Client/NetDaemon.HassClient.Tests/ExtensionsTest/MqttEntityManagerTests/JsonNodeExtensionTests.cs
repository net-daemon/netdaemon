using System.Collections;
using System.Text.Json.Nodes;
using NetDaemon.Extensions.MqttEntityManager.Helpers;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

public class JsonNodeExtensionTests
{
    /// <summary>
    /// Test data for simple key=value structs
    /// </summary>
    class SimpleMergeTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { J(new { a = "1", b = "2" }), J(new { }), J(new { a = "1", b = "2" }) };
            yield return new object[]
                { J(new { a = "1", b = "2" }), J(new { c = "3" }), J(new { a = "1", b = "2", c = "3" }) };
            yield return new object[] { J(new { a = "1", b = "2" }), J(new { a = "5" }), J(new { a = "5", b = "2" }) };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Test for simple key=value structs
    /// </summary>
    /// <param name="target"></param>
    /// <param name="toMerge"></param>
    /// <param name="expected"></param>
    [Theory]
    [ClassData(typeof(SimpleMergeTestData))]
    public void CanMergeSimpleDictionaries(JsonObject target, JsonObject? toMerge, JsonObject expected)
    {
        target.AddRange(toMerge);

        // Getting cyclic errors comparing the json structs so setup simple dictionaries to compare
        var targetDict = JsonSerializer.Deserialize<Dictionary<string, string>>(target.ToJsonString());
        var expectedDict = JsonSerializer.Deserialize<Dictionary<string, string>>(expected.ToJsonString());

        targetDict.Should().BeEquivalentTo(expectedDict);
    }

    [Fact]
    public void CanAddComplexToSimple()
    {
        var o1 = new { a = 1, b = 2, c = 3 };
        var o2 = new { d = new { x = 8, y = 9 } };

        var j1 = J(o1);

        j1.AddRange(J(o2));
        var combined = JsonConvert.DeserializeObject<dynamic>(j1.ToJsonString());
        Assert.Equal("1", combined.a.ToString());
        Assert.Equal("2", combined.b.ToString());
        Assert.Equal("3", combined.c.ToString());
        Assert.Equal("8", combined.d.x.ToString());
        Assert.Equal("9", combined.d.y.ToString());
    }

    [Fact]
    public void CanMergeSimpleIntoComplex()
    {
        var o1 = new { a = 1, b = 2, c = new { x = 8, y = 9 } };
        var o2 = new { a = 3 };

        var j1 = J(o1);

        j1.AddRange(J(o2));
        var combined = JsonConvert.DeserializeObject<dynamic>(j1.ToJsonString());
        Assert.Equal("3", combined.a.ToString());
        Assert.Equal("2", combined.b.ToString());
        Assert.Equal("8", combined.c.x.ToString());
        Assert.Equal("9", combined.c.y.ToString());
    }

    [Fact]
    public void CanMergeComplexIntoSimple()
    {
        var o1 = new { a = 1, b = 2, c = 3 };
        var o2 = new { c = new { x = 8, y = 9 } };

        var j1 = J(o1);

        j1.AddRange(J(o2));
        var combined = JsonConvert.DeserializeObject<dynamic>(j1.ToJsonString());
        Assert.Equal("1", combined.a.ToString());
        Assert.Equal("2", combined.b.ToString());
        Assert.Equal("8", combined.c.x.ToString());
        Assert.Equal("9", combined.c.y.ToString());
    }

    [Fact]
    public void CanMergeComplexIntoComplex()
    {
        var o1 = new { a = 1, b = new { q = 10, r = 11 }, c = 3 };
        var o2 = new { c = new { x = 8, y = 9 } };

        var j1 = J(o1);

        j1.AddRange(J(o2));
        var combined = JsonConvert.DeserializeObject<dynamic>(j1.ToJsonString());
        Assert.Equal("1", combined.a.ToString());
        Assert.Equal("10", combined.b.q.ToString());
        Assert.Equal("11", combined.b.r.ToString());
        Assert.Equal("8", combined.c.x.ToString());
        Assert.Equal("9", combined.c.y.ToString());
    }

    [Fact]
    public void CanMergeMultiLayerComplex()
    {
        var o1 = new { a = 1, b = 2, c = 3 };
        var o2 = new
        {
            person =
                new
                {
                    name = new
                    {
                        surname = "smith", forename = "john"
                    },
                    age = new
                    {
                        quantity = 11000, unit = "days"
                    }
                },
            id = new
            {
                style = "numeric",
                value = 1234
            }
        };

        var j1 = J(o1);
        j1.AddRange(J(o2));
        var combined = JsonConvert.DeserializeObject<dynamic>(j1.ToJsonString());
        
        Assert.Equal("1", combined.a.ToString());
        Assert.Equal("2", combined.b.ToString());
        Assert.Equal("3", combined.c.ToString());
        Assert.Equal("smith", combined.person.name.surname.ToString());
        Assert.Equal("john", combined.person.name.forename.ToString());
        Assert.Equal("11000", combined.person.age.quantity.ToString());
        Assert.Equal("days", combined.person.age.unit.ToString());
        Assert.Equal("numeric", combined.id.style.ToString());
        Assert.Equal("1234", combined.id.value.ToString());
    }


    private static JsonObject J(dynamic o)
    {
        return JsonSerializer.SerializeToNode(o);
    }
}