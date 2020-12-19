using System;
using System.IO;

namespace NetDaemon.Common.Configuration
{
    /// <summary>
    ///     Settings related to NetDaemon instance
    /// </summary>
    public class NetDaemonSettings
    {
        /// <summary>
        ///     Set true to generate entieies from Home Assistant
        /// </summary>
        public bool? GenerateEntities { get; set; } = false;
        /// <summary>
        ///     If Admin gui would be  used
        /// </summary>
        public bool? Admin { get; set; } = false;
        /// <summary>
        ///     Where the apps are found
        /// </summary>
        /// <remarks>
        ///     Can be ether a folder where the apps is found or
        ///     point to a csproj file or a dll precompiled daemon.
        ///     In the case it is not a folder, NetDaemon expects
        ///     the apps to be in the file paths and tries to find
        ///     all apps recursivly
        /// </remarks>
        public string? AppSource { get; set; } = null;

        /// <summary>
        ///     Returns the directory path of AppSource 
        /// </summary>
        public string GetAppSourceDirectory()
        {
            var source = AppSource?.Trim() ?? throw new NullReferenceException("AppSource cannot be null!");

            if (source.EndsWith(".csproj") || source.EndsWith(".dll"))
            {
                source = Path.GetDirectoryName(source);
            }

            return source ?? throw new NullReferenceException("Source cannot be null!");
        }
    }
}