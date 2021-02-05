using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    public  class WeatherEntity : RxEntityBase
    {

        public WeatherEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }
}