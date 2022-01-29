using System;
using System.Reflection;

namespace NetDaemon.Extensions.Scheduling.Tests;

/// <summary>
/// Helper class to fake timezone for localtime
/// </summary>
internal class FakeLocalTimeZone : IDisposable
{
    private readonly TimeZoneInfo _actualLocalTimeZoneInfo;

    private static void SetLocalTimeZone(TimeZoneInfo timeZoneInfo)
    {
        // Fake timezone by using reflection of private fields, this might break in the future
        var info = typeof(TimeZoneInfo).GetField("s_cachedData", BindingFlags.NonPublic | BindingFlags.Static);
        var cachedData = info!.GetValue(null);

        var field = cachedData!.GetType().GetField("_localTimeZone", BindingFlags.NonPublic | BindingFlags.Instance);
        field!.SetValue(cachedData, timeZoneInfo);
    }

    public FakeLocalTimeZone(TimeZoneInfo timeZoneInfo)
    {
        _actualLocalTimeZoneInfo = TimeZoneInfo.Local;
        SetLocalTimeZone(timeZoneInfo);
    }

    public void Dispose()
    {
        SetLocalTimeZone(_actualLocalTimeZoneInfo);
    }
}