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
    /// <param name="target"></param>
    /// <param name="toMerge"></param>
    public static void AddRange(this JsonObject target, JsonObject? toMerge)
    {
        foreach (var kvp in toMerge)
        {
            var k = kvp.Key;
            var v = JsonValue.Create(kvp.Value?.GetValue<object>());

            if (target.ContainsKey(k))
                target.Remove(k);
            
            target.Add(new(k, v));
        }
    }

}