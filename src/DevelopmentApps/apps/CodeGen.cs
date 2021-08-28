using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using NetDaemon.Common;

namespace NetDaemon.DevelopmentApps.apps
{
    [NetDaemonApp]
    public class CodeGen : IAsyncInitializable
    {
        private readonly IHassClient _hassClient;

        public CodeGen(IHassClient hassClient)
        {
            _hassClient = hassClient;
        }


        public async  Task InitializeAsync()
        {
            var entities = await _hassClient.GetEntities();
            foreach (var hassEntity in entities)
            {
            }
        }
    }
}