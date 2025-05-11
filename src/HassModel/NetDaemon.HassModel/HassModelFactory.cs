namespace NetDaemon.HassModel;

public class HassModelFactory
{
    public static IHaContext Create(IHomeAssistantRunner runner)
    {
        var collection = new ServiceCollection();
        collection.AddScopedHaContext();
        collection.AddSingleton(runner);
        return collection.BuildServiceProvider().GetRequiredService<IHaContext>();
    }
}
