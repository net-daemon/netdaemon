using System.Threading.Tasks;

namespace NetDaemon.Daemon.Storage
{
    public interface IDataRepository
    {
        /// <summary>
        ///     Saves data in a generic repository
        /// </summary>
        /// <param name="id">Unique id of data to save</param>
        /// <param name="data">The data to save</param>
        /// <typeparam name="T">The type of the data saved</typeparam>
        Task Save<T>(string id, T data);

        /// <summary>
        ///     Gets data from repositiry
        /// </summary>
        /// <typeparam name="T?">Type of data</typeparam>
        ValueTask<T?> Get<T>(string id) where T : class;
    }
}