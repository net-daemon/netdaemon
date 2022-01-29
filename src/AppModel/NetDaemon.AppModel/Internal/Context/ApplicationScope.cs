namespace NetDaemon.AppModel.Internal;

// Helper class to make ApplicationContext resolvable per scope
internal class ApplicationScope
{
    private ApplicationContext? _applicationContext;

    public ApplicationContext ApplicationContext
    {
        get => _applicationContext ??
                throw new InvalidOperationException("ApplicationScope.ApplicationContext has not been initialized yet");
        set => _applicationContext = value;
    }
}
