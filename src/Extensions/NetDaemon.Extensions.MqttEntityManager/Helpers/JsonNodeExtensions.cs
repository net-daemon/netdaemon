using System.Text.Json;
using System.Text.Json.Nodes;

namespace NetDaemon.Extensions.MqttEntityManager.Helpers;

/// <summary>
/// Extensions for JsonNode and inheritors
/// </summary>
internal static class JsonNodeExtensions
{
    /// <summary>
    /// For a given JsonObject, merge a second set of values, replacing any pre-existing properties in
    /// the target object
    /// </summary>
    /// <param name="target">The target JSON object.</param>
    /// <param name="toMerge">The JSON object to merge into the target.</param>
    public static void AddRange(this JsonObject target, JsonObject? toMerge)
    {
        if (toMerge == null)
            return;

        foreach (var kvp in toMerge)
        {
            var k = kvp.Key;
            var v = kvp.Value;

            target.Remove(k);

            target.Add(new(k, v?.Deserialize<JsonNode>()));
        }
    }

}
