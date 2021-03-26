using System.Collections.Generic;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public class SwitchEntity : RxEntityBase
    {
        /// <inheritdoc />
        public SwitchEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        /// Returns the Current Entity State of the object
        /// </summary>
        /// <inheritdoc/>
        public new IObservable<(SwitchEntityState Old, SwitchEntityState New)> StateAllChanges
        {
            get
            {
                return base.StateAllChanges.Select(t => (new SwitchEntityState(t.Old), new SwitchEntityState(t.New)));
            }
        }

        /// <inheritdoc/>
        public new IObservable<(SwitchEntityState Old, SwitchEntityState New)> StateChanges
        {
            get
            {
                return base.StateChanges.Select(t => (new SwitchEntityState(t.Old), new SwitchEntityState(t.New)));
            }
        }

        /// <summary>
        /// Returns true if the state equals on
        /// </summary>
        public bool IsOn
        {
            get
            {
                return EntityState?.IsOn ?? false;
            }
        }

        /// <summary>
        /// Returns true if the state equals off
        /// </summary>
        public bool IsOff
        {
            get
            {
                return EntityState?.IsOff ?? false;
            }
        }

        /// <summary>
        /// Returns true if the state is null. This means that home assisant cannot contact the devices
        /// </summary>
        public bool IsUnknown
        {
            get
            {
                return EntityState?.IsUnknown ?? true;
            }
        }
        /// <summary>
        /// Switch Entity state with Switch specific properties
        /// </summary>
        public override SwitchEntityState? EntityState
        {
            get
            {
                if (base.EntityState == null)
                {
                    return null;
                }
                return (SwitchEntityState)base.EntityState;
            }
        }
    }
    /// <summary>
    /// Custom switch entity state for Switches
    /// </summary>
    /// <value></value>
    public record SwitchEntityState : EntityState
    {
        public SwitchEntityState(EntityState currentEntity) : base(currentEntity)
        {

        }
        /// <summary>
        /// Returns true if it is on
        /// </summary>
        /// <value></value>
        public bool IsOn
        {
            get
            {
                return State == "on";
            }
        }

        /// <summary>
        /// Returns true if the state equals off
        /// </summary>
        public bool IsOff
        {
            get
            {
                return State == "off";
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
