namespace NetDaemon.HassModel.Entities
{
    /// <summary>
    /// The options related to Assist/Conversation Assistants
    /// </summary>
    public record ConversationOptions
    {
        /// <summary>
        /// If the entity should be available to voice/ai assistants
        /// </summary>
        public bool? ShouldExpose { get; init; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ConversationOptions() { }

        /// <summary>
        /// Conversion from Websocket/API Model
        /// </summary>
        /// <param name="entityConversationOpts"></param>
        public ConversationOptions(HassEntityConversationOptions entityConversationOpts)
        {
            ShouldExpose = entityConversationOpts.ShouldExpose;
        }
    }

}
