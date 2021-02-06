
using System;
using System.Collections.Generic;
using NetDaemon.Common.Configuration;

namespace NetDaemon.Common
{
    /// <summary>
    ///     Base class for all external events
    /// </summary>
    public class ExternalEventBase
    {
    }

    /// <summary>
    ///     Sent when app information are changed in the netdaemon
    /// </summary>
    public class AppsInformationEvent : ExternalEventBase
    {
    }
}