using System;
using System.Collections.Generic;
using System.Linq;

namespace NetDaemon.Common
{
    /// <summary>
    /// Marks a class with dependencies
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class DependsOnAttribute : Attribute
    {
        public DependsOnAttribute(params Type[] dependencies)
        {
            Dependencies = dependencies.Select(t => t.Name).ToArray();
        }

        public DependsOnAttribute(params string[] dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        /// Id of an app
        /// </summary>
        public IEnumerable<string> Dependencies { get; init; }
    }
}