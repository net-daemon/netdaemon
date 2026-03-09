using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("NetDaemon.Extensions.Scheduling.Tests")]
namespace NetDaemon.Extensions.Scheduler.SunEvents
{
    internal interface ISolarCalendar
    {
        DateTimeOffset Sunset { get; }
        DateTimeOffset Sunrise { get; }
        DateTimeOffset Dusk { get; }
        DateTimeOffset Dawn { get; }
    }
}
