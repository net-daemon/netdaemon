using System.Threading.Tasks;
using NetDaemon.Daemon;
namespace NetDaemon.Service.App.CodeGeneration
{
    public interface ICodeGenerationHandler
    {
        Task GenerateEntitiesAsync(NetDaemonHost daemonHost, string sourceFolder);
    }
}
