using System;
using System.Text;

namespace helpers.Logging.Loggers
{
    public class ConsoleLogger : LoggerBase
    {
        public static void UsePrettyLogging()
        {
            Console.OutputEncoding = Encoding.Unicode;
            Console.InputEncoding = Encoding.Unicode;

            LoggerBase.DebugTag = "ℹ️ DEBUG";
            LoggerBase.WarnTag = "⚠️ WARN";
            LoggerBase.InfoTag = "✅ INFO";
            LoggerBase.ErrorTag = "🛑 ERROR";
            LoggerBase.OpenBracketChar = '〘';
            LoggerBase.CloseBracketChar = '〙';
        }

        public static void DisablePrettyLogging()
        {
            LoggerBase.DebugTag = "DEBUG";
            LoggerBase.WarnTag = "WARN";
            LoggerBase.InfoTag = "INFO";
            LoggerBase.ErrorTag = "ERROR";
            LoggerBase.OpenBracketChar = '[';
            LoggerBase.CloseBracketChar = ']';

            Console.OutputEncoding = Encoding.Default;
            Console.InputEncoding = Encoding.Default;
        }

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