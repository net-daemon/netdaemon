using Innovative.SolarCalculator;

namespace NetDaemon.Extensions.Scheduler.SunEvents;

internal class SolarCalendar : ISolarCalendar
{
    private readonly SolarTimes _cachedCalculator;

    public SolarCalendar(Coordinates coordinates)
    {
        _cachedCalculator = new SolarTimes(DateTime.Now, coordinates.Latitude, coordinates.Longitude);
    }

    private SolarTimes SolarCalculator
    {
        get
        {
            _cachedCalculator.ForDate = DateTime.Now;
            return _cachedCalculator;
        }
    }

    public DateTimeOffset Sunset => SolarCalculator.Sunset;

    public DateTimeOffset Sunrise => SolarCalculator.Sunrise;

    public DateTimeOffset Dusk => SolarCalculator.DuskCivil;

    public DateTimeOffset Dawn => SolarCalculator.DawnCivil;
}
