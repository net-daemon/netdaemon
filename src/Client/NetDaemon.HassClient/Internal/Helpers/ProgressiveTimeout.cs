namespace NetDaemon.Client.Internal.Helpers;

/// <summary>
///  Progeressively increases timeout from a min value to a max value with a step
/// </summary>
public class ProgressiveTimeout
{
    private readonly int _initialTimeout;
    private readonly int _maxTimeout;
    private readonly double _increaseFactor;
    private int _currentTimeout;

    public ProgressiveTimeout(int initialTimeout, int maxTimeout, double increaseFactor)
    {
        if (initialTimeout <= 0)
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

    public int GetNextTimeout()
    {
        int timeout = _currentTimeout;
        _currentTimeout = Math.Min((int)(_currentTimeout * _increaseFactor), _maxTimeout);
        return timeout;
    }

    public void Reset()
    {
        _currentTimeout = _initialTimeout;
    }
}
