using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using NetDaemon.Common.Fluent;
using NetDaemon.Common.Reactive;

namespace NetDaemon.Common.Services
{
    public  class ZoneEntity : RxEntityBase
    {
 
        public ZoneEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void Reload(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("zone", "reload", serviceData);
        }
    }
}