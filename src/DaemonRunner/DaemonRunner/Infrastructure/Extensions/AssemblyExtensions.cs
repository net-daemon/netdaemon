using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetDaemon.Infrastructure.Extensions
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetTypesWhereSubclassOf<T>(this Assembly assembly)
        {
            return assembly.GetTypes().Where(x => x.IsClass && x.IsSubclassOf(typeof(T)));
        }
    }
}