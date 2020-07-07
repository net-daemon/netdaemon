namespace NetDaemon.Common.Fluent
{
    /// <summary>
    /// 
    /// </summary>
    public class Context
    {
        /// <summary>
        ///     Id
        /// </summary>
        public string Id { get; set; } = "";
        /// <summary>
        /// 
        /// </summary>
        public string? ParentId { get; set; }
        /// <summary>
        ///     The id of the user who is responsible for the connected item.
        /// </summary>
        public string? UserId { get; set; }

    }
}