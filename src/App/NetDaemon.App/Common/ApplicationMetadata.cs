using System;

namespace NetDaemon.Common
{
    internal class ApplicationMetadata : IApplicationMetadata
    {
        public ApplicationMetadata(Type appType)
        {
            AppType = appType;
        }

        public string? Id { get; set; }
        public string? Description { get; set; }
        public bool IsEnabled { get; set; } = true;
        public AppRuntimeInfo RuntimeInfo { get; } = new ();
        public Type AppType { get;  }
    }
}