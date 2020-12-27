using System;
using System.Collections.Generic;

namespace NetDaemon.Service.App
{
    public interface IDaemonAppCompiler
    {
        /// <summary>
        /// Temporary
        /// </summary>
        /// <returns></returns>
        IEnumerable<Type> GetApps();
    }
}