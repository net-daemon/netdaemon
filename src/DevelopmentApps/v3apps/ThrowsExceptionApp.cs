using System;
using NetDaemon.AppModel;
using NetDaemon.HassModel.Common;

namespace AppModelApps;

[NetDaemonApp]
public class ThrowInConstructor
{
    public ThrowInConstructor(
        IHaContext ha
    )
    {
        throw new InvalidOperationException("Constructor error");
    }
}