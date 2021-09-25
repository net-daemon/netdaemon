using System;
using NetDaemon.Common;

namespace NetDaemon.DevelopmentApps.apps
{
    [Focus]
    [NetDaemonApp]
    public sealed class PersistApp : IInitializable, IDisposable
    {
        public PersistApp()
        {
            Console.WriteLine("Ctor");
        }

        public void Initialize()
        {
            Console.WriteLine("Initialize");
        }

        public void Dispose()
        {
            Console.WriteLine("Dispose");
        }
    }
}