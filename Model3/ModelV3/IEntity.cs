using System;

namespace NetDaemon.Common.Model
{
    public interface IEntity<TEntityState>  where TEntityState : IEntityProperties
    {
        /// <summary>
        ///     Observable, All state changes inkluding attributes
        /// </summary>
        IObservable<(TEntityState Old, TEntityState New)> StateAllChanges { get; }

        /// <summary>
        ///     Observable, All state changes. New.State!=Old.State
        /// </summary>
        IObservable<(TEntityState Old, TEntityState New)> StateChanges { get; }

        /// <summary>
        /// Gets the state of this Entity
        /// </summary>
        TEntityState? EntityState { get; }

        /// <summary>
        ///     Calls a service using current entity id/s and the entity domain
        /// </summary>
        /// <param name="service">Name of the service to call</param>
        /// <param name="data">Data to provide</param>
        /// <param name="waitForResponse">Waits for Home Assistant to return result before returning</param>
        void CallService(string service, object? data = null, bool waitForResponse = false);
    }

    public interface IToggleEntity<TEntityState> : IEntity<TEntityState> where TEntityState : IEntityProperties
    {
        /// <summary>
        ///     Toggles state on/off on entity
        /// </summary>
        /// <param name="attributes">The attributes to set.</param>
        void Toggle(object? attributes = null) => CallService("toggle", attributes);

        /// <summary>
        ///     Turn off entity
        /// </summary>
        /// <param name="attributes">The attributes to set.</param>
        void TurnOff(object? attributes = null) => CallService("turn_off", attributes);

        /// <summary>
        ///     Turn on entity
        /// </summary>
        /// <param name="attributes">The attributes to set.</param>
        void TurnOn(object? attributes = null) => CallService("turn_on", attributes);
    }
}