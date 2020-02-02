iex "del D:\GIT\netdaemon\src\App\NetDaemon.App\bin\Debug\*.nupkg"
iex "del D:\GIT\netdaemon\src\Daemon\NetDaemon.Daemon\bin\Debug\*.nupkg"
iex "del D:\GIT\netdaemon\src\DaemonRunner\bin\Debug\*.nupkg"

iex "dotnet pack ..\src\"

iex "del D:\GIT\daemonapp\packs\JoySoftware.NetDaemon.App\*.nupkg"
iex "cp D:\GIT\netdaemon\src\App\NetDaemon.App\bin\Debug\*.nupkg D:\GIT\daemonapp\packs\JoySoftware.NetDaemon.App\"

iex "del D:\GIT\daemonapp\packs\JoySoftware.NetDaemon.Daemon\*.nupkg"
iex "cp D:\GIT\netdaemon\src\Daemon\NetDaemon.Daemon\bin\Debug\*.nupkg D:\GIT\daemonapp\packs\JoySoftware.NetDaemon.Daemon\"

iex "del D:\GIT\daemonapp\packs\JoySoftware.NetDaemon.DaemonRunner\*.nupkg"
iex "cp D:\GIT\netdaemon\src\DaemonRunner\bin\Debug\*.nupkg D:\GIT\daemonapp\packs\JoySoftware.NetDaemon.DaemonRunner\"
