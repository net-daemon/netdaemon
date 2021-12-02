using System;
using System.Collections.Generic;
using System.Reflection;

namespace NetDaemon.Service.App
{
    public interface IDaemonAssemblyCompiler
    {
        /// <summary>
        /// Temporary
        /// </summary>
        IEnumerable<Assembly> Load();
    }
}