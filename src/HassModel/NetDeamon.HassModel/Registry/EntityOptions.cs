namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Details of Options in the Home Assistant Registry
/// </summary>
public record EntityOptions
{
    /// <summary>
    /// The Assist/Conversation options for a registration entity
    /// </summary>
    public ConversationOptions? ConversationOptions { get; init; }
}

