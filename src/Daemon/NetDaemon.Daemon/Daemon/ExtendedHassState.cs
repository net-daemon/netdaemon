using System.Globalization;
using JoySoftware.HomeAssistant.Model;

namespace NetDaemon.Daemon
{
    /// <summary>
    /// Helper for storing parsed state value in the StateManager
    /// </summary>
    /// <remarks>
    /// <para>
    /// The state in HASS is actually always a string. previously HassClient tried to be smart and determine the actual
    /// type and put it in the HassState.State as dynamic. This logic has been removed from HassClient but ND still
    /// needs it for compatibility reasons.
    /// </para><para>
    /// Moreover in certain situations ND will update the state in a stateChanged event and it will need to save the
    /// updated state in the stateManger for when it is later retrieved.
    /// We solve this by deriving from HassSate and add an extra property ObjectState to save the parsed state.
    /// </para>
    /// </remarks>
    internal record ExtendedHassState : HassState
    {
        /// <summary>
        /// Copy constructor from HassState
        /// </summary>
        public ExtendedHassState(HassState hassState) : base(hassState)
        {
            ObjectState = ParseDataType(hassState.State);
        }
        public  object? ObjectState { get; set; }

        private static object? ParseDataType(string? state)
        {
            if (long.TryParse(state, NumberStyles.Number, CultureInfo.InvariantCulture, out long intValue))
                return intValue;

            if (double.TryParse(state, NumberStyles.Number, CultureInfo.InvariantCulture, out double doubleValue))
                return doubleValue;

            if (state == "unavailable")
                return null;

            return state;
        }
    }
}