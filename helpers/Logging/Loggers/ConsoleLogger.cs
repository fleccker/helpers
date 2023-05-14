using System;

namespace helpers.Logging.Loggers
{
    public class ConsoleLogger : LoggerBase
    {
        public override void Log(LogBuilder log)
        {
            while (log.TryGetNext(out var next))
            {
                Console.ForegroundColor = next.Item1;
                Console.Write(next.Item2);
                Console.ResetColor();
            }

            Console.Write('\n');

            base.Log(log);
        }
    }
}