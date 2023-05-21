using System.Threading;

namespace helpers.Extensions
{
    [LogSource("Timer Extensions")]
    public static class TimerExtensions
    {
        public static void Start(this Timer timer, int callPeriod)
        {
            timer.Change(0, callPeriod);
        }
    }
}