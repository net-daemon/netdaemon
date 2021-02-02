using NetDaemon.Common.Reactive;
using NetDaemon.Common.Services;

// Use unique namespaces for your apps if you going to share with others to avoid
// conflicting names
namespace NetDaemon.DevelopmentApps.apps.HelloWorld
{
    /// <summary>
    ///     The NetDaemonApp implements System.Reactive API
    ///     currently the default one
    /// </summary>
    public class HelloWorldApp : NetDaemonRxApp
    {
        public LightEntity MyEntity { get; set; }

        public override void Initialize()
        {
            MyEntity.TurnOn();
            
            Log("{@entity}", MyEntity);
        }
    }
}