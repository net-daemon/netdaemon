using System;

namespace NetDaemon.Common.ModelV3
{
    public static class EntityExtensions
    {
        public static bool IsOn(this EntityState entity) => string.Equals(entity?.State, "on", StringComparison.OrdinalIgnoreCase);
        
        public static bool IsOff(this EntityState entity) => string.Equals(entity?.State, "off", StringComparison.OrdinalIgnoreCase);

        public static bool IsOn(this Entity entity) => entity?.EntityState?.IsOn() ?? false;
        
        public static bool IsOff(this Entity entity) => entity?.EntityState?.IsOff() ?? false;

    }
}