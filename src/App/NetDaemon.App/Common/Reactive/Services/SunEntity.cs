using System.Collections.Generic;
using System;
using System.Reactive;
using System.Reactive.Linq;

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
            get { return EntityState?.Elevation ?? -100; }
        }
        /// <summary>
        /// Returns the Current Entity State of the object
        /// </summary>
        /// <inheritdoc/>
        public new IObservable<(SunEntityState Old, SunEntityState New)> StateAllChanges
        {
            get
            {
                return base.StateAllChanges.Select(t => (new SunEntityState(t.Old), new SunEntityState(t.New)));
            }
        }

        /// <inheritdoc/>
        public new IObservable<(SunEntityState Old, SunEntityState New)> StateChanges
        {
            get
            {
                return base.StateChanges.Select(t => (new SunEntityState(t.Old), new SunEntityState(t.New)));
            }
        }
        /// <inheritdoc/>
        public override SunEntityState? EntityState
        {
            get
            {
                if (base.EntityState == null)
                {
                    return null;
                }
                return (SunEntityState)base.EntityState;
            }
        }
    }

/// <summary>
/// Entity state that is dedicated to Sun specific states
/// </summary>
/// <value></value>
public record SunEntityState : EntityState
{
    /// <summary>
    /// Create a new Sun Entity
    /// </summary>
    /// <param name="currentEntity">Current entity to copy</param>
    /// <returns></returns>
    public SunEntityState(EntityState currentEntity) : base(currentEntity)
    {

    }
    /// <summary>
    /// Read the elevation from the attributes of the sun entity. Returns -100 if anything is null or missing
    /// </summary>
    /// <value></value>
    public double Elevation
    {
        get { return Attribute?.elevation ?? -100; }
    }
}
}