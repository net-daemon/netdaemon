namespace NetDaemon.Extensions.MqttEntityManager.Helpers;

/// <summary>
/// Parsing utilities for entity IDs
/// </summary>
internal static class EntityIdParser
{
    /// <summary>
    /// Extract the domain and identifier from an entity ID string
    /// </summary>
    /// <param name="entityId">Entity ID in the format "domain.identifier"</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">If entityId is not supplied or is an invalid
    /// format</exception>
    public static (string domain, string identifier) Extract(string entityId)
    {
        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException(nameof(entityId));

        var components = entityId.Split('.', 2);
        if (components.Length != 2 ||
            string.IsNullOrWhiteSpace(components[0]) ||
            string.IsNullOrWhiteSpace(components[1]))
            throw new ArgumentException(
                $"The {nameof(entityId)} should be of the format 'domain.identifier'. The value was {entityId}");

        return (components[0], components[1]);
    }
}