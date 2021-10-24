using System;
using NetDaemon.Common;
using NetDaemon.HassModel.Common;

namespace NetDaemon.DevelopmentApps.apps
{
    [NetDaemonApp]
    public class Isolation1
    {
        public Isolation1(IHaContext ha)
        {
            Console.WriteLine("isolator 1");
            ha.StateAllChanges.Subscribe(e =>
            {
                Console.WriteLine("app 1.1");
                throw new Exception("Exception 1.1");
            });
            ha.StateAllChanges.Subscribe(e =>
            {
                Console.WriteLine("app 1.2");
            });            
            
        }
    }
    
    [NetDaemonApp]
    public class Isolation2
    {
        public Isolation2(IHaContext ha)
        {
            Console.WriteLine("isolator 2");
            ha.StateAllChanges.Subscribe(e =>
            {
                Console.WriteLine("app 2");

                //throw new Exception();
            });
        }
    }
    
    [NetDaemonApp]
    public class Isolation3
    {
        public Isolation3(IHaContext ha)
        {
            Console.WriteLine("isolator 2");
            ha.StateAllChanges.Subscribe(e =>
            {
                Console.WriteLine("app 3");
            });
        }
    }
}