using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NetDaemon;
using Serilog;
using TestClient;

namespace TestClient
{
    public static class MainProgram
    {
        public static readonly CancellationTokenSource GlobalCancellationSource = new();
        public static async Task Main(string[] args)
        {
            try
            {
                await Host.CreateDefaultBuilder(args)
                    .UseDefaultNetDaemonLogging()
                    .UseNetDaemon()
                    .UseIntegrationTest()
                    .Build()
                    .RunAsync(GlobalCancellationSource.Token)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to start host... {e}");
                throw;
            }
            finally
            {
                CleanupNetDaemon();
            }
        }
        private static void CleanupNetDaemon()
        {
            Log.CloseAndFlush();
        }
    }
}


