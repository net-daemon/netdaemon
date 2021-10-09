using NetDaemon.Model3.Entities;

namespace NetDaemon.Model3.Common
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