using helpers.Pooling.Pools;

using System;

namespace helpers.Logging
{
    public class LoggerBase
    {
        public void Debug(object message) => Debug(helpers.Log.ResolveCaller(1), message);
        public void Info(object message) => Info(helpers.Log.ResolveCaller(1), message);
        public void Warn(object message) => Warn(helpers.Log.ResolveCaller(1), message);
        public void Error(object message) => Error(helpers.Log.ResolveCaller(1), message);
        public void Log(object message, ConsoleColor? color = null) => Log(LogBuilderPool.Pool.Get().WithLine(message, color));

        public virtual void Info(object source, object message)
        {
            Log(new LogBuilder()
                .WithLine("<<", ConsoleColor.Cyan)
                .WithLine("INFO", ConsoleColor.White)
                .WithLine(">> ", ConsoleColor.Cyan)

                .WithLine("<<", ConsoleColor.Cyan)
                .WithLine(DateTime.UtcNow.ToString("T"), ConsoleColor.White)
                .WithLine(">> ", ConsoleColor.Cyan)

                .WithLine("<<", ConsoleColor.Cyan)
                .WithLine(source, ConsoleColor.DarkCyan)
                .WithLine(">> ", ConsoleColor.Cyan)

                .WithLine(message, ConsoleColor.White));
        }

        public virtual void Warn(object source, object message)
        {
            Log(new LogBuilder()
                .WithLine("<<", ConsoleColor.Yellow)
                .WithLine("WARN", ConsoleColor.White)
                .WithLine(">> ", ConsoleColor.Yellow)

                .WithLine("<<", ConsoleColor.Yellow)
                .WithLine(DateTime.UtcNow.ToString("T"), ConsoleColor.White)
                .WithLine(">> ", ConsoleColor.Yellow)

                .WithLine("<<", ConsoleColor.Yellow)
                .WithLine(source, ConsoleColor.DarkYellow)
                .WithLine(">> ", ConsoleColor.Yellow)

                .WithLine(message, ConsoleColor.White));
        }

        public virtual void Error(object source, object message)
        {
            Log(new LogBuilder()
                .WithLine("<<", ConsoleColor.Red)
                .WithLine("ERROR", ConsoleColor.White)
                .WithLine(">> ", ConsoleColor.Red)

                .WithLine("<<", ConsoleColor.Red)
                .WithLine(DateTime.UtcNow.ToString("T"), ConsoleColor.White)
                .WithLine(">> ", ConsoleColor.Red)

                .WithLine("<<", ConsoleColor.Red)
                .WithLine(source, ConsoleColor.DarkRed)
                .WithLine(">> ", ConsoleColor.Red)

                .WithLine(message, ConsoleColor.White));
        }

        public virtual void Debug(object source, object message)
        {
            Log(new LogBuilder()
                .WithLine("<<", ConsoleColor.Blue)
                .WithLine("DEBUG", ConsoleColor.White)
                .WithLine(">> ", ConsoleColor.Blue)

                .WithLine("<<", ConsoleColor.Blue)
                .WithLine(DateTime.UtcNow.ToString("T"), ConsoleColor.White)
                .WithLine(">> ", ConsoleColor.Blue)

                .WithLine("<<", ConsoleColor.Blue)
                .WithLine(source, ConsoleColor.DarkBlue)
                .WithLine(">> ", ConsoleColor.Blue)

                .WithLine(message, ConsoleColor.White));
        }

        public virtual void Log(LogBuilder log)
        {

        }
    }
}