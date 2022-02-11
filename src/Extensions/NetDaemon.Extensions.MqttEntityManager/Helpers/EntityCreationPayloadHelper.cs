using System.Text.Json;
using System.Text.Json.Nodes;
using NetDaemon.Extensions.MqttEntityManager.Models;

namespace NetDaemon.Extensions.MqttEntityManager.Helpers;

/// <summary>
/// Helpers around EntityCreationPayload
/// </summary>
internal class EntityCreationPayloadHelper
{
    /// <summary>
    /// Merge an optional dynamic set of parameters with the concrete payload
    /// </summary>
    /// <param name="concreteOptions"></param>
    /// <param name="dynamicOptions"></param>
    /// <returns></returns>
    /// <exception cref="JsonException"></exception>
    internal static string Merge(EntityCreationPayload concreteOptions, dynamic? dynamicOptions)
    {
        var concreteJson = JsonSerializer.SerializeToNode(concreteOptions)?.AsObject()
                           ?? throw new JsonException("Unable to convert concrete config to JsonObject");

        if (dynamicOptions != null)
        {
            JsonObject dynamicJson = JsonSerializer.SerializeToNode(dynamicOptions);
            concreteJson.AddRange(dynamicJson);
        }

        return concreteJson.ToJsonString();
    }
}