namespace NetDaemon.Client.Internal.Helpers;

/// <summary>
///     Used to implement tracking of result messages in the background
/// </summary>
internal interface IResultMessageHandler
{
    /// <summary>
    ///     Track the result message and do proper logging of timeouts and errors
    /// </summary>
    /// <param name="returnMessageTask">Task of a resulting return message</param>
    /// <param name="originalCommand">The original command that we are tracking result of</param>
    void HandleResult(Task<HassMessage> returnMessageTask, CommandMessage originalCommand);
}
