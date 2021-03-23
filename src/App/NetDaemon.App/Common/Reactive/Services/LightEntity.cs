using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public class LightEntity : RxEntityBase
    {
        /// <inheritdoc />
        public LightEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        /// Returns true if the state equals on
        /// </summary>
        public bool IsOn
        {
            get
            {
                return (State ?? "Unknown") == "on";
            }
        }

        /// <summary>
        /// Returns true if the state equals off
        /// </summary>
        public bool IsOff
        {
            get
            {
                return (State ?? "Unknown") == "off";
            }
        }

        /// <summary>
        /// Returns true if the state is null. This means that home assisant cannot contact the devices 
        /// </summary>
        public bool IsUnknown
        {
            get
            {
                return State == null;
            }
        }
    }
}