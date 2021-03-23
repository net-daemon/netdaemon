using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public class SunEntity : RxEntityBase
    {
        /// <inheritdoc />
        public SunEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }
        /// <summary>
        /// Initialize using the Home Assistant Default
        /// </summary>
        /// <param name="daemon"></param>
        /// <returns></returns>
        public SunEntity(INetDaemonRxApp daemon) : this(daemon, new[] { "sun.sun" })
        {
        }
        /// <summary>
        /// Read the elevation from the attributes of the sun entity. Returns -100 if anything is null or missing
        /// </summary>
        public double Elevation
        {
            get { return Attribute?.elevation ?? -100; }
        }



    }
}