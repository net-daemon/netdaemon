using System.Collections.Generic;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace NetDaemon.Common.Reactive.Services
{

    public record BinaryEntityState : EntityState
    {
        public BinaryEntityState(EntityState currentEntity) : base(currentEntity)
        {

        }
        /// <summary>
        /// Read the elevation from the attributes of the sun entity. Returns -100 if anything is null or missing
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

    /// <inheritdoc />
    public class BinarySensorEntity : RxEntityBase
    {
        /// <inheritdoc />
        public BinarySensorEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        ///  Returns the Current Entity State of the object
        /// </summary>
        /// <inheritdoc/>
        public new IObservable<(BinaryEntityState Old, BinaryEntityState New)> StateAllChanges
        {
            get
            {
                return base.StateAllChanges.Select(t => (new BinaryEntityState(t.Old), new BinaryEntityState(t.New)));
            }
        }

        /// <inheritdoc/>
        public new IObservable<(BinaryEntityState Old, BinaryEntityState New)> StateChanges
        {
            get
            {
                return base.StateChanges.Select(t => (new BinaryEntityState(t.Old), new BinaryEntityState(t.New)));
            }
        }

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


        public bool IsOn => EntityState.IsOn;
        public bool IsOff => EntityState.IsOff;
        public bool IsUnknown => EntityState.IsUnknown;
    }
}
 