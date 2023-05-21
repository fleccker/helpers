using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;

namespace helpers
{
    [LogSource("Exception Manager")]
    public static class ExceptionManager
    {
        private static List<Exception> _exceptionStore = new List<Exception>();
        private static List<Exception> _unhandledExceptionStore = new List<Exception>();

        private static Exception _lastException;
        private static Exception _lastUnhandledException;

        public static event Action<Exception> OnThrown;
        public static event Action<Exception> OnUnhandled;

        public static IReadOnlyList<Exception> ExceptionStore { get => _exceptionStore; }
        public static IReadOnlyList<Exception> UnhandledExceptionStore { get => _unhandledExceptionStore; }

        public static Exception LastException { get => _lastException; }
        public static Exception LastUnhandledException { get => _lastUnhandledException; }

        public static StackTrace Trace { get => new StackTrace(); }

        public static IReadOnlyList<StackFrame> Frames
        {
            get => Trace
                       .GetFrames()
                       .Where(x => x.GetMethod().DeclaringType != typeof(ExceptionManager))
                       .ToList();
        }

        public static IReadOnlyList<MethodBase> StackMethods { get => Frames.Select(x => x.GetMethod()).ToList(); }
        public static IReadOnlyList<Type> StackTypes { get => StackMethods.Select(x => x.DeclaringType).ToList(); }

        public static string LogPath { get; set; }
        public static string UnhandledLogPath { get; set; }

        public static bool LogAll { get; set; }
        public static bool UnhandledLogAll { get; set; }

        public static bool LogToConsole { get; set; }

        public static bool Any<T>() where T : Exception
            => _exceptionStore.Any(x => x is T);

        public static void Load()
        {
            if (AppDomain.CurrentDomain != null)
            {
                AppDomain.CurrentDomain.FirstChanceException += OnException;
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

                helpers.Log.Debug($"Installed safe exception handlers.");
            }

            helpers.Log.Info("Enabled exception logging.");

            LogAll = true;
            UnhandledLogAll = true;
            LogToConsole = true;

            LogPath = $"{Directory.GetCurrentDirectory()}/exception_log_full.txt";
            UnhandledLogPath = $"{Directory.GetCurrentDirectory()}/exception_log_unhandled.txt";

            helpers.Log.Debug($"Logging all exceptions in {LogPath}");
            helpers.Log.Debug($"Logging unhandled exceptions in {UnhandledLogPath}");
        }

        public static void Unload()
        {
            if (AppDomain.CurrentDomain != null)
            {
                AppDomain.CurrentDomain.FirstChanceException -= OnException;
                AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;

                _exceptionStore.Clear();
                _unhandledExceptionStore.Clear();

                helpers.Log.Debug("Removed safe exception handlers.");
            }

            helpers.Log.Info("Disabled exception logging.");
        }

        public static void Clear()
        {
            _exceptionStore.Clear();
            _unhandledExceptionStore.Clear();

            helpers.Log.Debug("Cleared exception store.");
        }

        public static void ExecuteSafe(Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                helpers.Log.Debug($"Caught an exception when executing {action.Method.Name}");
                helpers.Log.Debug(ex);
            }
        }

        public static void ExecuteSafe(Action action, params object[] args)
        {
            try
            {
                action?.Method?.Invoke(action.Target, args);
            }
            catch
            {

            }
        }

        public static void ExecuteSafe(Type type, string name, params object[] args)
        {
            var method = type.GetMethod(name);

            if (method != null)
            {
                try
                {
                    method.Invoke(null, args);
                }
                catch { }
            }
        }

        public static void ExecuteSafe(Type type, string name, object handle, params object[] args)
        {
            var method = type.GetMethod(name);

            if (method != null)
            {
                try
                {
                    method.Invoke(handle, args);
                }
                catch { }
            }
        }

        public static void ExecuteSafe<T>(string name, params object[] args)
            => ExecuteSafe(typeof(T), name, args);

        public static void ExecuteSafe<T>(string name, T handle, params object[] args)
            => ExecuteSafe(typeof(T), name, handle, args);

        public static T ExecuteSafe<T>(Func<T> function)
        {
            try
            {
                return function.Invoke();
            }
            catch
            {
                return default;
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs ev)
        {
            if (ev.ExceptionObject != null && ev.ExceptionObject is Exception exception)
            {
                _lastUnhandledException = exception;
                _unhandledExceptionStore.Add(exception);

                try { OnUnhandled?.Invoke(exception); } catch { }

                Log(true, exception);
            }
        }

        private static void OnException(object sender, FirstChanceExceptionEventArgs ev)
        {
            _lastException = ev.Exception;
            _exceptionStore.Add(ev.Exception);

            try { OnThrown?.Invoke(ev.Exception); } catch { }

            Log(false, ev.Exception);
        }

        public static void Log(bool isUnhandled, Exception exception)
        {
            try
            {
                var excStr = ExceptionToString(exception, true);

                if (LogToConsole)
                {
                    helpers.Log.Error($"Caught an exception!");
                    helpers.Log.Error(excStr);
                }

                if (isUnhandled)
                {
                    if (UnhandledLogAll)
                    {
                        if (!File.Exists(UnhandledLogPath))
                            File.WriteAllText(UnhandledLogPath, excStr);
                        else
                            File.AppendAllText(UnhandledLogPath, excStr);
                    }
                    else
                    {
                        File.WriteAllText(UnhandledLogPath, excStr);
                    }
                }
                else
                {
                    if (LogAll)
                    {
                        if (!File.Exists(LogPath))
                            File.WriteAllText(LogPath, excStr);
                        else
                            File.AppendAllText(LogPath, excStr);
                    }
                    else
                    {
                        File.WriteAllText(LogPath, excStr);
                    }
                }
            }
            catch { }
        }

        public static string ExceptionToString(Exception exception, bool isTop)
        {
            var sb = new StringBuilder();

            if (isTop)
                sb.AppendLine($"---- Exception thrown at {DateTime.Now.ToString("G")} ----");

            try
            {
                sb.AppendLine(
                    $"Type: {exception.GetType().FullName}\n" +
                    $"Result: {exception.HResult}\n" +
                    $"Source: {exception.Source ?? "Unknown"}\n" +
                    $"Message: {exception.Message}");
            }
            catch { }

            sb.AppendLine();
            sb.AppendLine("Stack Trace:");
            sb.AppendLine(StackToString(Trace, true));

            if (isTop)
            {
                sb.AppendLine("------------------------------");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static string StackToString(StackTrace trace, bool includeProcess)
        {
            var sb = new StringBuilder();
            var stack = trace.GetFrames();

            sb.AppendLine($"---- Stack start: {stack.Length} frames -----");

            if (includeProcess)
            {
                try
                {
                    var process = Process.GetCurrentProcess();
                    var thread = Thread.CurrentThread;

                    sb.AppendLine(
                        $"-> Process Information <-\n" +
                        $"  >- ID: {process.Id}\n" +
                        $"  >- Name: {process.ProcessName}\n" +
                        $"  -> Memory: {process.WorkingSet64}\n" +
                        $"-----------------------");

                    sb.AppendLine();
                    sb.AppendLine(
                        $"-> Thread Information <-\n" +
                        $"  >- ID: {thread.ManagedThreadId}\n" +
                        $"  -> Name: {thread.Name}\n" +
                        $"  -> Priority: {thread.Priority}\n" +
                        $"-------------------------");

                    sb.AppendLine();
                }
                catch { }
            }

            for (int i = 0; i < stack.Length; i++)
            {
                var frame = stack[i];

                var method = frame.GetMethod();
                var type = method.DeclaringType;
                var assembly = type.Assembly;

                try
                {
                    sb.AppendLine($"" +
                        $"-> Frame: {i + 1} / {stack.Length}\n" +
                        $"  >- Method: {method.Name}\n" +
                        $"  >- Module: {method.Module.Name}\n" +
                        $"  >- Type: {type.FullName}\n" +
                        $"  >- Assembly: {assembly.FullName}\n" +
                        $"  >- Assembly Path: {assembly.CodeBase}");
                }
                catch { }

                sb.AppendLine();
            }

            sb.AppendLine($"---- Stack end ----");

            return sb.ToString();
        }
    }
}