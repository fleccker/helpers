using helpers.Pooling.Pools;

using System;

namespace helpers.Logging
{
    public class LoggerBase
    {
        public static char OpenBracketChar = '〘';
        public static char CloseBracketChar = '〙';

        public static string InfoTag = "✅ INFO";
        public static string WarnTag = "⚠️ WARN";
        public static string ErrorTag = "🛑 ERROR";
        public static string DebugTag = "ℹ️ DEBUG";

        public void Debug(object message) => Debug(helpers.Log.ResolveCaller(1), message);
        public void Info(object message) => Info(helpers.Log.ResolveCaller(1), message);
        public void Warn(object message) => Warn(helpers.Log.ResolveCaller(1), message);
        public void Error(object message) => Error(helpers.Log.ResolveCaller(1), message);
        public void Log(object message, ConsoleColor? color = null) => Log(LogBuilderPool.Pool.Get().WithLine(message, color));

        public virtual void Info(object source, object message)
        {
            Log(new LogBuilder()
                .WithLine(OpenBracketChar, ConsoleColor.Cyan)
                .WithLine(InfoTag, ConsoleColor.White)
                .WithLine(CloseBracketChar, ConsoleColor.Cyan)

                .WithLine(OpenBracketChar, ConsoleColor.Cyan)
                .WithLine(DateTime.UtcNow.ToString("T"), ConsoleColor.White)
                .WithLine(CloseBracketChar, ConsoleColor.Cyan)

                .WithLine(OpenBracketChar, ConsoleColor.Cyan)
                .WithLine(source, ConsoleColor.DarkCyan)
                .WithLine(CloseBracketChar, ConsoleColor.Cyan)

                .WithLine(message, ConsoleColor.White));
        }

        public virtual void Warn(object source, object message)
        {
            Log(new LogBuilder()
                .WithLine(OpenBracketChar, ConsoleColor.Yellow)
                .WithLine(WarnTag, ConsoleColor.White)
                .WithLine(CloseBracketChar, ConsoleColor.Yellow)

                .WithLine(OpenBracketChar, ConsoleColor.Yellow)
                .WithLine(DateTime.UtcNow.ToString("T"), ConsoleColor.White)
                .WithLine(CloseBracketChar, ConsoleColor.Yellow)

                .WithLine(OpenBracketChar, ConsoleColor.Yellow)
                .WithLine(source, ConsoleColor.DarkYellow)
                .WithLine(CloseBracketChar, ConsoleColor.Yellow)

                .WithLine(message, ConsoleColor.White));
        }

        public virtual void Error(object source, object message)
        {
            Log(new LogBuilder()
                .WithLine(OpenBracketChar, ConsoleColor.Red)
                .WithLine(ErrorTag, ConsoleColor.White)
                .WithLine(CloseBracketChar, ConsoleColor.Red)

                .WithLine(OpenBracketChar, ConsoleColor.Red)
                .WithLine(DateTime.UtcNow.ToString("T"), ConsoleColor.White)
                .WithLine(CloseBracketChar, ConsoleColor.Red)

                .WithLine(OpenBracketChar, ConsoleColor.Red)
                .WithLine(source, ConsoleColor.DarkRed)
                .WithLine(CloseBracketChar, ConsoleColor.Red)

                .WithLine(message, ConsoleColor.White));
        }

        public virtual void Debug(object source, object message)
        {
            Log(new LogBuilder()
                .WithLine(OpenBracketChar, ConsoleColor.Blue)
                .WithLine(DebugTag, ConsoleColor.White)
                .WithLine(CloseBracketChar, ConsoleColor.Blue)

                .WithLine(OpenBracketChar, ConsoleColor.Blue)
                .WithLine(DateTime.UtcNow.ToString("T"), ConsoleColor.White)
                .WithLine(CloseBracketChar, ConsoleColor.Blue)

                .WithLine(OpenBracketChar, ConsoleColor.Blue)
                .WithLine(source, ConsoleColor.DarkBlue)
                .WithLine(CloseBracketChar, ConsoleColor.Blue)

                .WithLine(message, ConsoleColor.White));
        }

        public virtual void Log(LogBuilder log)
        {

        }
    }
}