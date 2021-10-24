using NetDaemon.HassModel.Entities;

namespace NetDaemon.HassModel.Common
{
    /// <summary>
    /// Extension methods for HaContext
    /// </summary>
    public static class HaContextExtensions
    {
        /// <summary>
        /// Creates a new Entity instance
        /// </summary>
        public static Entity Entity(this IHaContext haContext, string entityId) => new (haContext, entityId);
    }
}