Invoke-Expression "del D:\GIT\netdaemon\src\App\NetDaemon.App\bin\Debug\*.nupkg"
Invoke-Expression "del D:\GIT\netdaemon\src\Daemon\NetDaemon.Daemon\bin\Debug\*.nupkg"
Invoke-Expression "del D:\GIT\netdaemon\src\DaemonRunner\DaemonRunner\bin\Debug\*.nupkg"

Invoke-Expression "dotnet pack ..\src\"

Invoke-Expression "del D:\GIT\daemonapp\packs\JoySoftware.NetDaemon.App\*.nupkg"
Invoke-Expression "cp D:\GIT\netdaemon\src\App\NetDaemon.App\bin\Debug\*.nupkg D:\GIT\daemonapp\packs\JoySoftware.NetDaemon.App\"
Invoke-Expression "del D:\GIT\daemontest\cs\package\JoySoftware.NetDaemon.App\*.nupkg"
Invoke-Expression "cp D:\GIT\netdaemon\src\App\NetDaemon.App\bin\Debug\*.nupkg D:\GIT\daemontest\cs\package\JoySoftware.NetDaemon.App\"

Invoke-Expression "del D:\GIT\daemonapp\packs\JoySoftware.NetDaemon.Daemon\*.nupkg"
Invoke-Expression "cp D:\GIT\netdaemon\src\Daemon\NetDaemon.Daemon\bin\Debug\*.nupkg D:\GIT\daemonapp\packs\JoySoftware.NetDaemon.Daemon\"
Invoke-Expression "del D:\GIT\daemontest\cs\package\JoySoftware.NetDaemon.Daemon\*.nupkg"
Invoke-Expression "cp D:\GIT\netdaemon\src\Daemon\NetDaemon.Daemon\bin\Debug\*.nupkg D:\GIT\daemontest\cs\package\JoySoftware.NetDaemon.Daemon\"

Invoke-Expression "del D:\GIT\daemonapp\packs\JoySoftware.NetDaemon.DaemonRunner\*.nupkg"
Invoke-Expression "cp D:\GIT\netdaemon\src\DaemonRunner\DaemonRunner\bin\Debug\*.nupkg D:\GIT\daemonapp\packs\JoySoftware.NetDaemon.DaemonRunner\"
Invoke-Expression "del D:\GIT\daemontest\cs\package\JoySoftware.NetDaemon.DaemonRunner\*.nupkg"
Invoke-Expression "cp D:\GIT\netdaemon\src\DaemonRunner\DaemonRunner\bin\Debug\*.nupkg D:\GIT\daemontest\cs\package\JoySoftware.NetDaemon.DaemonRunner\"
