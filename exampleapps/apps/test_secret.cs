using System.Threading.Tasks;
using System.Linq;
using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using Netdaemon.Generated.Reactive;
using NetDaemon.Common;
using NetDaemon.Common.Reactive;

namespace NStest
{

    /// <summary> Test the secrets </summary>
    public class SecretApp : NetDaemonRxApp

    {
        public long? SomeNum { get; set; }
        public override async Task InitializeAsync()
        {
            RunEvery(TimeSpan.FromSeconds(5), () => Log("App is working...{secret}", SomeNum));

        }

    }

}
