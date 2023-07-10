using helpers.Extensions;
using helpers.Logging;
using helpers.Logging.Loggers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace helpers
{
    public static class Log
    {
        private static Dictionary<Type, string> _attributedSources = new Dictionary<Type, string>();

        private static List<LogLevel> _blacklistLevels = new List<LogLevel>();
        private static List<LoggerBase> _loggers = new List<LoggerBase>();

        private static List<string> _librarySources = new List<string>();
        private static List<string> _blacklistedSources = new List<string>();

        private static Func<MethodBase, Type, string> _sourceResolver;

        static Log()
        {
            foreach (var type in Assembly
                .GetExecutingAssembly()
                .GetTypes())
            {
                if (type.TryGetAttribute<LogSourceAttribute>(out var logSourceAttribute))
                {
                    AddLibrarySource(logSourceAttribute.GetSource());
                }

                foreach (var method in type.GetMethods())
                {
                    if (method.TryGetAttribute(out logSourceAttribute))
                    {
                        AddLibrarySource(logSourceAttribute.GetSource());
                    }
                }
            }
        }

        public static void ClearSourceCache()
            => _attributedSources.Clear();

        public static void AddLibrarySource(string librarySource)
            => _librarySources.Add(librarySource);

        public static bool IsBlacklisted(LogLevel logLevel)
            => _blacklistLevels.Contains(logLevel);

        public static bool IsBlacklisted(string source)
            => _blacklistedSources.Contains(source);

        public static void ClearSourceBlacklist()
            => _blacklistedSources.Clear();

        public static void ClearLevelBlacklist()
            => _blacklistLevels.Clear();

        public static void Blacklist(LogLevel logLevel)
            => _blacklistLevels.Add(logLevel);

        public static void Blacklist(string source)
            => _blacklistedSources.Add(source);

        public static void Unblacklist(LogLevel logLevel)
            => _blacklistLevels.Remove(logLevel);

        public static void Unblacklist(string source)
            => _blacklistedSources.Remove(source);

        public static void BlacklistLibrarySources()
            => _blacklistedSources.AddRange(_librarySources);

        public static void AddLogger(LoggerBase loggerBase)
            => _loggers.Add(loggerBase);

        public static void AddLogger<T>() where T : LoggerBase, new()
            => _loggers.Add(new T());

        public static void AddLoggers(params LoggerBase[] loggers)
            => _loggers.AddRange(loggers);

        public static void RemoveLogger(LoggerBase loggerBase)
            => _loggers.Remove(loggerBase);

        public static void RemoveLogger<T>() where T : LoggerBase
            => _loggers.ForEach(x => _loggers.Remove(x), y => y is T);

        public static void RemoveLoggers(params LoggerBase[] loggers)
            => loggers.ForEach(x => _loggers.Remove(x));

        public static void RemoveAllLoggers()
            => _loggers.Clear();

        public static void DisableFileLoggers()
            => _loggers.ForEach(x => (x as FileLogger).Disable(), y => y is FileLogger);

        public static void EnableFileLoggers()
            => _loggers.ForEach(x => (x as FileLogger).Enable(), y => y is FileLogger);

        public static void ToggleFileLoggers()
            => _loggers.ForEach(x => (x as FileLogger).Toggle(), y => y is FileLogger);

        public static void SetSourceResolver(Func<MethodBase, Type, string> resolver)
            => _sourceResolver = resolver;

        public static void Error(object message) => Show(LogLevel.Error, null, message);
        public static void Error(object source, object message) => Show(LogLevel.Error, source, message);

        public static void Warn(object message) => Show(LogLevel.Warn, null, message);
        public static void Warn(object source, object message) => Show(LogLevel.Warn, source, message);

        public static void Debug(object message) => Show(LogLevel.Debug, null, message);
        public static void Debug(object source, object message) => Show(LogLevel.Debug, source, message);

        public static void Info(object message) => Show(LogLevel.Info, null, message);
        public static void Info(object source, object message) => Show(LogLevel.Info, source, message);

        public static void Show(LogLevel level, object source, object message)
        {
            var sourceStr = source?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(sourceStr)) 
                sourceStr = ResolveCaller(1);

            _loggers.ForEach(x =>
            {
                switch (level)
                {
                    case LogLevel.Debug:
                        x.Debug(sourceStr, message);
                        break;

                    case LogLevel.Error:
                        x.Error(sourceStr, message);
                        break;

                    case LogLevel.Warn:
                        x.Warn(sourceStr, message);
                        break;

                    case LogLevel.Info:
                        x.Info(sourceStr, message);
                        break;
                }
            });
        }

        public static string Build(this LogBuilder logBuilder)
        {
            string str = "";

            while (logBuilder.TryGetNext(out var line))
            {
                str += $"{line.Item2} ";
            }

            return str.TrimEnd();
        }

        public static string ResolveCaller(int framesToSkip = 0)
        {
            var method = GetCallingMethod(framesToSkip);

            if (_attributedSources.TryGetValue(method.DeclaringType, out var source)) return source;
            if (method.DeclaringType.TryGetAttribute<LogSourceAttribute>(out var sourceAttribute) || method.TryGetAttribute(out sourceAttribute))
                source = sourceAttribute.GetSource();
            else
            {
                if (_sourceResolver != null) source = _sourceResolver(method, method.DeclaringType);
                else source = method.DeclaringType.Name.SpaceByPascalCase();
            }

            _attributedSources[method.DeclaringType] = source;
            return source;
        }

        private static MethodBase GetCallingMethod(int framesToSkip)
        {
            var trace = new StackTrace(framesToSkip);

            foreach (var frame in trace.GetFrames())
            {
                var method = frame.GetMethod();
                if (method.DeclaringType == typeof(Log)) continue;
                return method;
            }

            return null;
        }
    }
}