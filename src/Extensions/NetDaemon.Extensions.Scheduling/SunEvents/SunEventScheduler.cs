using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("NetDaemon.Extensions.Scheduling.Tests")]

namespace NetDaemon.Extensions.Scheduler.SunEvents;

internal sealed class SunEventScheduler : ISunEventScheduler
{
    private readonly IScheduler _reactiveScheduler;
    private readonly ISolarCalendar _solarCalendar;

    public SunEventScheduler(ISolarCalendar solarCalendar, IScheduler reactiveScheduler)
    {
        _reactiveScheduler = reactiveScheduler;
        _solarCalendar = solarCalendar;
    }
    
    internal IDisposable RunAtSunEvent(Func<DateTimeOffset> getSunEventTime, Action action)
    {
        var todaysSunEvent = getSunEventTime().ToLocalTime();
        var now = _reactiveScheduler.Now.ToLocalTime();
        var tomorrow = now.Date.AddDays(1);

        //Only schedule if the sun event is still going to occur today, the cron schedule will take over from tomorrow
        if (todaysSunEvent > now && todaysSunEvent < tomorrow)
        {
            _reactiveScheduler.Schedule(todaysSunEvent, action);
        }

        return _reactiveScheduler.ScheduleCron("0 0 * * *", () =>
        {
            _reactiveScheduler.Schedule(getSunEventTime(), action);
        });
    }

    /// <inheritdoc/>
    public IDisposable RunAtSunset(Action action)
    {
        return RunAtSunEvent(() => _solarCalendar.Sunset, action);
    }

    /// <inheritdoc/>
    public IDisposable RunAtDawn(Action action)
    {
        return RunAtSunEvent(() => _solarCalendar.Dawn, action);
    }

    /// <inheritdoc/>
    public IDisposable RunAtSunrise(Action action)
    {
        return RunAtSunEvent(() => _solarCalendar.Sunrise, action);
    }

    /// <inheritdoc/>
    public IDisposable RunAtDusk(Action action)
    {
        return RunAtSunEvent(() => _solarCalendar.Dusk, action);
    }
}
