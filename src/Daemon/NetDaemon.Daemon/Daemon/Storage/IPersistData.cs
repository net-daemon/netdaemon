using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Daemon.Storage
{
    public interface IDataRepository
    {
        Task Save<T>(string id, T data);

        ValueTask<T?> Get<T>(string id) where T : class;
    }
}