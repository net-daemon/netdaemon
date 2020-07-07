using JoySoftware.HomeAssistant.Client;
using NetDaemon.Common.Fluent;

namespace NetDaemon.Mapping
{
    public static class ContextMapper
    {
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