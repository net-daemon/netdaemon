using System.Collections.Generic;
namespace NetDaemon.Model3.Entities
{
    public class Target
    {
        public Target()
        {
        }

        public Target(string entityId)
        {
            EntityIds = new[]{ entityId };
        }

        public IReadOnlyCollection<string>? EntityIds { get; init; }

        public IReadOnlyCollection<string>? DeviceIds { get; init; }

        public IReadOnlyCollection<string>? AreaIds { get; init; }
    }
}