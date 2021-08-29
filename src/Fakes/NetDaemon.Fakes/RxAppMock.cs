using Microsoft.Extensions.Logging;
using Moq;
using NetDaemon.Common;
using NetDaemon.Common.Reactive;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using Newtonsoft.Json;

namespace NetDaemon.Daemon.Fakes
{
    /// <summary>
    ///     Mock of RxApp to test your own applications using
    ///     a separate implementations class
    /// </summary>
    public class RxAppMock : RxAppMock<INetDaemonRxApp>
    {
    }
}