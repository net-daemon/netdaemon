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

    /// <summary>
    /// Default Constructor
    /// </summary>
    public EntityOptions() { }

    /// <summary>
    /// Conversion from Websocket/API Model
    /// </summary>
    /// <param name="entityOpts"></param>
    public EntityOptions(HassEntityOptions? entityOpts)
    {
        if (entityOpts?.Conversation == null)
            ConversationOptions = new ConversationOptions();
        else
            ConversationOptions = new ConversationOptions(entityOpts.Conversation);
                
    }
}

