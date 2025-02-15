namespace NetDaemon.HassModel.Entities;

/// <summary>
/// The options related to Assist/Conversation Assistants
/// </summary>
public record ConversationOptions
{
    /// <summary>
    /// If the entity should be available to voice/ai assistants
    /// </summary>
    public bool? ShouldExpose { get; init; }
}
