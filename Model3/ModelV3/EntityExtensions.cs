using System;

namespace NetDaemon.Common.ModelV3
{
    public static class EntityExtensions
    {
        public static bool IsOn(this Entity entity) => string.Equals(entity.State, "on", StringComparison.OrdinalIgnoreCase);
        
        public static bool IsOff(this Entity entity) => string.Equals(entity.State, "off", StringComparison.OrdinalIgnoreCase);
    }
}