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

        //I want a way to create sun entity without remember the homeAssistant magic string of sun.sun. But this wont compile. I would love some suggestions.
        // public SunEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds = new List<string>(){"sun.sun"}) : base(daemon, entityIds)
        // {

        // }
        /// <summary>
        /// Read the elevation from the attributes of the sun entity. Returns -100 if anything is null or missing
        /// </summary>
        public double Elevation
        {
            get { return Attribute?.elevation ?? -100; }
        }



    }
}