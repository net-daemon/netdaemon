using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    public interface IScheduler
    {
        Task RunEveryAsync(int millisecondsDelay, Func<Task> func);
        Task RunEveryAsync(TimeSpan timeSpan, Func<Task> func);
        Task RunInAsync(int millisecondsDelay, Func<Task> func);
        Task RunInAsync(TimeSpan timeSpan, Func<Task> func);
        void RunEvery(int millisecondsDelay, Func<Task> func);
        void RunEvery(TimeSpan timeSpan, Func<Task> func);
        void RunIn(int millisecondsDelay, Func<Task> func);
        void RunIn(TimeSpan timeSpan, Func<Task> func);

    }
}
