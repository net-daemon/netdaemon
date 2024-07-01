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
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(initialTimeout, TimeSpan.Zero, nameof(initialTimeout));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxTimeout, initialTimeout, nameof(maxTimeout));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(increaseFactor, 1, nameof(increaseFactor));

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
