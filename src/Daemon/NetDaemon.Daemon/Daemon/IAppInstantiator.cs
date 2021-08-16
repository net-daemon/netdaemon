using System;
using NetDaemon.Common;

namespace NetDaemon.Daemon
{
    public interface IAppInstantiator
    {
        ApplicationContext Instantiate(Type netDaemonAppType, string appId);
    }
}