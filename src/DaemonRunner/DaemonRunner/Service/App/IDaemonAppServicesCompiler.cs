using System;
using System.Collections.Generic;
using System.Reflection;

namespace NetDaemon.Service.App
{
    public interface IDaemonAppServicesCompiler
    {
        /// <summary>
        /// Temporary
        /// </summary>
        IEnumerable<Type> GetAppServices(IEnumerable<Assembly> assemblies);
    }
}