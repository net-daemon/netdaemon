using System;

namespace NetDaemon.Common.Model
{
    /// <summary>
    ///     IRxEntityBase interface represents what you can do on
    ///     Entity("").WhatYouCanDo(); and Entities(n=> n.Xyz).WhatYouCanDo();
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        ///     Set entity state
        /// </summary>
        /// <param name="state">The state to set, primitives only</param>
        /// <param name="attributes">The attributes to set. Use anonomous type</param>
        /// <param name="waitForResponse">Waits for Home Assistant to return result before returning</param>
        void SetState(dynamic state, dynamic? attributes = null, bool waitForResponse = false);

        /// <summary>
        ///     Observable, All state changes inkluding attributes
        /// </summary>
        IObservable<(IEntityProperties Old, IEntityProperties New)> StateAllChanges { get; }

        /// <summary>
        ///     Observable, All state changes. New.State!=Old.State
        /// </summary>
        IObservable<(IEntityProperties Old, IEntityProperties New)> StateChanges { get; }

        /// <summary>
        ///     Calls a service using current entity id/s and the entity domain
        /// </summary>
        /// <param name="service">Name of the service to call</param>
        /// <param name="data">Data to provide</param>
        /// <param name="waitForResponse">Waits for Home Assistant to return result before returning</param>
        void CallService(string service, object? data = null, bool waitForResponse = false);
    }

    public interface IToggleEntity : IEntity
    {
        /// <summary>
        ///     Toggles state on/off on entity
        /// </summary>
        /// <param name="attributes">The attributes to set. Use anonomous type</param>
        void Toggle(object? attributes = null);

        /// <summary>
        ///     Turn off entity
        /// </summary>
        /// <param name="attributes">The attributes to set. Use anonomous type.</param>
        void TurnOff(object? attributes = null);

        /// <summary>
        ///     Turn on entity
        /// </summary>
        /// <param name="attributes">The attributes to set. Use anonomous type.</param>
        void TurnOn(object? attributes = null);
    }
}