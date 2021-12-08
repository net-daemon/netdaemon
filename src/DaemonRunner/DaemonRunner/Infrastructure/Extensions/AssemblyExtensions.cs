using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetDaemon.Common;

namespace NetDaemon.Infrastructure.Extensions
{
    internal static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetAppClasses(this Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(type => type.IsClass && 
                               !type.IsGenericType && 
                               !type.IsAbstract && 
                               (type.IsAssignableTo(typeof(INetDaemonAppBase)) || type.GetCustomAttribute<NetDaemonAppAttribute>() != null
                               ));
        }
        
        public static IEnumerable<Type> GetAppServicesClasses(this Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(type => type.IsClass && 
                               !type.IsGenericType && 
                               !type.IsAbstract && 
                               type.GetCustomAttribute<NetDaemonServicesProviderAttribute>() != null
                               );
        }
    }
}