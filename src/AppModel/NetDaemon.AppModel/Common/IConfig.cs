namespace NetDaemon.AppModel.Common;

/// <summary>
///     Configuration in a app
/// </summary>
/// <typeparam name="T">Type of class representing the config</typeparam>
public interface IAppConfig<T> : IOptions<T> where T : class, new()
{
}