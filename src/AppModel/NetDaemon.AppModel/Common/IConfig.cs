namespace NetDaemon.AppModel;

/// <summary>
///     Configuration in a app
/// </summary>
/// <typeparam name="T">Type of class representing the config</typeparam>
public interface IAppConfig<out T> : IOptions<T> where T : class, new()
{
}
