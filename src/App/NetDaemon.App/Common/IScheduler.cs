using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    public interface IScheduler
    {
        Task RunEvery(int millisecondsDelay, Func<Task> func);
        Task RunEvery(TimeSpan timeSpan, Func<Task> func);
        Task RunIn(int millisecondsDelay, Func<Task> func);
        Task RunIn(TimeSpan timeSpan, Func<Task> func);
    }
}
