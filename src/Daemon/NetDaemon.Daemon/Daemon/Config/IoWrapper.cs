using System.IO;

namespace NetDaemon.Daemon.Config
{
    public class IoWrapper : IIoWrapper
    {
        public string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }
    }
}