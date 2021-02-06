using System;
using System.Collections.Generic;

namespace NetDaemon.Service.App
{
    public interface IDaemonAppCompiler
    {
        /// <summary>
        /// Temporary
        /// </summary>
        IEnumerable<Type> GetApps();
    }
}