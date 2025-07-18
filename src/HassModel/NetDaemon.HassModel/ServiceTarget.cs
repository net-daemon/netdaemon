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
        new() { EntityIds = [.. entityIds] };

    /// <summary>
    /// Override low level object Equals.  This is used by fluent assertions and other libraries to compare objects 
    /// </summary>
    /// <param name="obj">The object that we are comparing to</param>
    /// <returns>true if the two objects are the same (Reference equal), or if the two objects are ServiceTargets and passes the Equal test</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is null)
        {
            return false;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        ServiceTarget other = (ServiceTarget)obj;
        //return Equals((ServiceTarget)obj);
        return AreCollectionsEqual(EntityIds, other.EntityIds)
            && AreCollectionsEqual(DeviceIds, other.DeviceIds)
            && AreCollectionsEqual(AreaIds, other.AreaIds)
            && AreCollectionsEqual(FloorIds, other.FloorIds)
            && AreCollectionsEqual(LabelIds, other.LabelIds);

    }

    private static bool AreCollectionsEqual(IReadOnlyCollection<string>? a, IReadOnlyCollection<string>? b)
    {
        if (ReferenceEquals(a, b))
            return true;
        if (a is null || b is null)
            return false;
        bool bReturn = a.Count == b.Count && !a.Except(b).Any();
        return bReturn;
    }

    /// <summary>
    /// Override the GetHashCode for completeness for equality checking
    /// </summary>
    /// <returns>integer hash code for this ServiceTarget</returns>
    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 23 + (EntityIds is null ? 0 : GetCollectionHashCode(EntityIds));
        hash = hash * 23 + (DeviceIds is null ? 0 : GetCollectionHashCode(DeviceIds));
        hash = hash * 23 + (AreaIds is null ? 0 : GetCollectionHashCode(AreaIds));
        hash = hash * 23 + (FloorIds is null ? 0 : GetCollectionHashCode(FloorIds));
        hash = hash * 23 + (LabelIds is null ? 0 : GetCollectionHashCode(LabelIds));
        return hash;
    }

    private static int GetCollectionHashCode(IReadOnlyCollection<string> collection)
    {
        unchecked
        {
            int hash = 19;
            foreach (var item in collection.OrderBy(x => x))
            {
                hash = hash * 31 + item.GetHashCode(StringComparison.Ordinal);
            }
            return hash;
        }
    }


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

    /// <summary>
    /// Ids of floors to invoke a service on
    /// </summary>
    public IReadOnlyCollection<string>? FloorIds { get; init; }

    /// <summary>
    /// Ids of labels to invoke a service on
    /// </summary>
    public IReadOnlyCollection<string>? LabelIds { get; init; }

}
