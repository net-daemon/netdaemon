using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.Model;

namespace NetDaemon.Daemon
{
    public interface IHandleHassEvent
    {
        void Initialize(IHassClient client);
        Task HandleNewEvent(HassEvent hassEvent);
    }
}