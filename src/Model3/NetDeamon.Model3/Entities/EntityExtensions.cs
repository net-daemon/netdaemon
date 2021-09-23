using System;

namespace NetDaemon.Model3.Entities
{
    /// <summary>
    /// Provides Extension methods for Entities
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// Checks if en EntityState has the state "on" 
        /// </summary>
        /// <param name="entityState">The state to check</param>
        /// <returns>true if the state equals "on", otherwise false</returns>
        public static bool IsOn(this EntityState entityState) => string.Equals(entityState?.State, "on", StringComparison.OrdinalIgnoreCase);
        
        /// <summary>
        /// Checks if en EntityState has the state "off" 
        /// </summary>
        /// <param name="entityState">The state to check</param>
        /// <returns>true if the state equals "off", otherwise false</returns>
        public static bool IsOff(this EntityState entityState) => string.Equals(entityState?.State, "off", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Checks if en Entity has the state "on" 
        /// </summary>
        /// <param name="entity">The state to check</param>
        /// <returns>true if the state equals "on", otherwise false</returns>
        public static bool IsOn(this Entity entity) => entity?.EntityState?.IsOn() ?? false;
        
        /// <summary>
        /// Checks if en Entity has the state "off" 
        /// </summary>
        /// <param name="entity">The state to check</param>
        /// <returns>true if the state equals "off", otherwise false</returns>
        public static bool IsOff(this Entity entity) => entity?.EntityState?.IsOff() ?? false;
    }
}