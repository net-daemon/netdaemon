using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public class BinarySensorEntity : RxEntityBase
    {
        /// <inheritdoc />
        public BinarySensorEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        public bool IsOn
        {
            get
            {
                return (State ?? "Unknown") == "on";
            }
        }
        public bool IsOff
        {
            get
            {
                return (State ?? "Unknown") == "off";
            }
        }
        public bool IsUnknown
        {
            get
            {
                return State  == null;
            }
        }
    }
}