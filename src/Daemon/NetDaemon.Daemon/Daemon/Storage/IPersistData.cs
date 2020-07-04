using System.Threading.Tasks;

namespace NetDaemon.Daemon.Storage
{
    public interface IDataRepository
    {
        Task Save<T>(string id, T data);

        ValueTask<T?> Get<T>(string id) where T : class;
    }
}