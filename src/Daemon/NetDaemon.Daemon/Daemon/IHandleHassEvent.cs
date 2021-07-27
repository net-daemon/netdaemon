using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Model;

namespace NetDaemon.Daemon
{
    public interface IHandleHassEvent
    {
        Task HandleNewEvent(HassEvent hassEvent);
    }
}