namespace NetDaemon.Client.Internal.Helpers;

/// <summary>
///  Progeressively increases timeout from a min value to a max value with a step
/// </summary>
public class ProgressiveTimeout
{
    private readonly TimeSpan _initialTimeout;
    private readonly TimeSpan _maxTimeout;
    private readonly double _increaseFactor;
    private TimeSpan _currentTimeout;

    public ProgressiveTimeout(TimeSpan initialTimeout, TimeSpan maxTimeout, double increaseFactor)
    {
        if (initialTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(initialTimeout), "Initial timeout must be greater than zero.");

        if (maxTimeout < initialTimeout)
            throw new ArgumentOutOfRangeException(nameof(maxTimeout), "Max timeout must be greater than or equal to initial timeout.");

        if (increaseFactor <= 1)
            throw new ArgumentOutOfRangeException(nameof(increaseFactor), "Increase factor must be greater than 1.");

        _initialTimeout = initialTimeout;
        _maxTimeout = maxTimeout;
        _increaseFactor = increaseFactor;
        _currentTimeout = initialTimeout;
    }

    public TimeSpan Timeout
    {
        get
        {
            var timeout = _currentTimeout;
            var progressTimeout = _currentTimeout * _increaseFactor;
            _currentTimeout = progressTimeout > _maxTimeout ? _maxTimeout : progressTimeout;
            return timeout;
        }
    }

    public void Reset()
    {
        _currentTimeout = _initialTimeout;
    }
}
