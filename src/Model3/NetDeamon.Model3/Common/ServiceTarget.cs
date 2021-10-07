using System.Collections.Generic;
namespace NetDaemon.Model3.Entities
{
    /// <summary>
    /// Target for a service call
    /// </summary>
    public class ServiceTarget
    {
        /// <summary>
        /// Creates a new ServiceTarget from an EntityId
        /// </summary>
        /// <param name="entityId">The Id of teh entity</param>
        /// <returns>A new ServiceTarget</returns>
        public static ServiceTarget FromEntity(string entityId) =>
            new() { EntityIds = new[]{ entityId } };
        
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
}