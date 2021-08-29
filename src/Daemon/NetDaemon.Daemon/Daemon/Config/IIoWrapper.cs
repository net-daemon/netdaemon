namespace NetDaemon.Daemon.Config
{
    public interface IIoWrapper
    {
        string ReadFile(string path);
    }
}