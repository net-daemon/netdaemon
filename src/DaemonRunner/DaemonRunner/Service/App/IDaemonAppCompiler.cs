using System;
using System.Collections.Generic;
using System.Reflection;

namespace NetDaemon.Service.App
{
    public interface IDaemonAppCompiler
    {
        /// <summary>
        /// Temporary
        /// </summary>
        /// <returns></returns>
        [Obsolete("Only exists while migrating the world to IOC.")]
        IEnumerable<Type> GetApps();
        Assembly Load();
    }
}