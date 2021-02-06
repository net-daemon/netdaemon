using JoySoftware.HomeAssistant.Client;
using NetDaemon.Common;

namespace NetDaemon.Mapping
{
    public static class ContextMapper
    {
        /// <summary>
        ///     Maps HassContext to Context
        /// </summary>
        /// <param name="hassContext">The HassContext to map</param>
        public static Context Map(HassContext? hassContext)
        {
            if (hassContext == null)
                return new Context();

            return new Context
            {
                Id = hassContext.Id,
                ParentId = hassContext.ParentId,
                UserId = hassContext.UserId
            };
        }
    }
}