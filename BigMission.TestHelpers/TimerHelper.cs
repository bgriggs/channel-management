using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BigMission.TestHelpers
{
    public class TimerHelper : ITimerHelper
    {
        private Timer timer;

        public Timer Create(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            timer = new Timer(callback, state, dueTime, period);
            return timer;
        }

        public void Dispose()
        {
            timer.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            return timer.DisposeAsync();
        }
    }
}
