using System;

namespace helpers.Logging.Loggers
{
    public class CustomLogger : LoggerBase
    {
        public Action<LogBuilder> LogMethod { get; }

        public CustomLogger(Action<LogBuilder> log)
        {
            LogMethod = log;
        }

        public override void Log(LogBuilder log)
        {
            LogMethod?.Invoke(log);

            base.Log(log);
        }
    }
}