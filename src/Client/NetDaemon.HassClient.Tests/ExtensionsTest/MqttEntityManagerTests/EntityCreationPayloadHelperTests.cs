using System.Collections;
using NetDaemon.Extensions.MqttEntityManager.Helpers;
using NetDaemon.Extensions.MqttEntityManager.Models;
using Newtonsoft.Json.Linq;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

public class EntityCreationPayloadHelperTests
{
    sealed class PayloadTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new EntityCreationPayload(), new { },
                J(new { state_topic = (object)null!, json_attributes_topic = (object)null!, object_id = (object)null! }),
                "should output basic concrete options"
            };

            yield return new object[]
            {
                new EntityCreationPayload {}, new { extra = "data" },
                J(new { state_topic = (object)null!, json_attributes_topic = (object)null!, object_id = (object)null!, extra = "data" }), "should merge a new property"
            };

            yield return new object[]
            {
                new EntityCreationPayload { StateTopic = "/state"}, new {},
                J(new { state_topic = "/state", json_attributes_topic = (object)null!, object_id = (object)null!  }), "should pick up values"
            };

            yield return new object[]
            {
                new EntityCreationPayload { StateTopic = "/state"}, new { state_topic = "/new-state"},
                J(new { state_topic = "/new-state", json_attributes_topic = (object)null!, object_id = (object)null!  }), "should override concrete values"
            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Theory]
    [ClassData(typeof(PayloadTestData))]
    internal void MergeTests(EntityCreationPayload concreteOptions, object? additionalOptions, JToken expected, string because)
    {
        JToken mergedData = JToken.Parse(EntityCreationPayloadHelper.Merge(concreteOptions, additionalOptions));

        mergedData.Should().BeEquivalentTo(expected, because);
    }


    private static JToken J(object o)
    {
        return  JToken.Parse(JsonSerializer.Serialize(o));
    }
}
