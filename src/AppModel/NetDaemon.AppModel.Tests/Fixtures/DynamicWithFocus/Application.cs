using NetDaemon.AppModel;

namespace Apps;

[NetDaemonApp]
public class NonFocusApp
{
    public NonFocusApp()
    {
    }
}

[NetDaemonApp]
[Focus]
public class MyFocusApp
{
    public MyFocusApp()
    {
    }
}