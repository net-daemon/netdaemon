namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Target for a service call
/// </summary>
public class ServiceTarget
{
    /// <summary>
    /// Creates a new ServiceTarget from an EntityId
    /// </summary>
    /// <param name="entityId">The Id of the entity</param>
    /// <returns>A new ServiceTarget</returns>
    public static ServiceTarget FromEntity(string entityId) =>
        new() { EntityIds = new[] { entityId } };

    /// <summary>
    /// Creates a new ServiceTarget from a list of EntityIds
    /// </summary>
    /// <param name="entityIds">The Ids of entities</param>
    /// <returns>A new ServiceTarget</returns>
    public static ServiceTarget FromEntities(IEnumerable<string> entityIds) =>
        new() { EntityIds = entityIds.ToArray() };

    /// <summary>
    /// Creates a new ServiceTarget from EntityIds
    /// </summary>
    /// <param name="entityIds">The Ids of entities</param>
    /// <returns>A new ServiceTarget</returns>
    public static ServiceTarget FromEntities(params string[] entityIds) =>
        new() { EntityIds = entityIds.ToArray() };

    /// <summary>
    /// Creates a new empty ServiceTarget
    /// </summary>
    public ServiceTarget()
    { }

    /// <summary>
    /// IDs of entities to invoke a service on
    /// </summary>
    public IReadOnlyCollection<string>? EntityIds { get; init; }

    /// <summary>
    /// Ids of Devices to invoke a service on
    /// </summary>
    public IReadOnlyCollection<string>? DeviceIds { get; init; }

    /// <summary>
    /// Ids of Areas to invoke a service on
    /// </summary>
    public IReadOnlyCollection<string>? AreaIds { get; init; }
}