namespace NetDaemon.Common
{
    /// <summary>
    ///     Context
    /// </summary>
    public class Context
    {
        /// <summary>
        ///     Id
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once PropertyCanBeMadeInitOnly.Global
        public string Id { get; set; } = "";
        /// <summary>
        ///     ParentId
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once PropertyCanBeMadeInitOnly.Global
        public string? ParentId { get; set; }
        /// <summary>
        ///     The id of the user who is responsible for the connected item.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once PropertyCanBeMadeInitOnly.Global
        public string? UserId { get; set; }
    }
}