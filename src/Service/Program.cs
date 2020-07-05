using System.Threading.Tasks;
using NetDaemon.Service;

namespace Service
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            await Runner.Run(args).ConfigureAwait(false);
        }
    }
}