namespace LocalAppsWithErrors;

[NetDaemonApp]
public class MyAppLocalAppWithError
{
    public MyAppLocalAppWithError()
    {
        throw new InvalidOperationException("Some error");
    }
}
