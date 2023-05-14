using helpers.IO;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace helpers.Logging.Loggers
{
    public class FileLogger : LoggerBase
    {
        private Timer _timer;
        private FileLoggerMode _mode = FileLoggerMode.AppendToFile;
        private int _flushInterval;

        private List<string> _buffer = new List<string>();

        public FileLogger(FileLoggerMode mode, int interval, string filePath = null)
        {
            _mode = mode;
            _flushInterval = interval;

            FilePath = filePath;

            if (string.IsNullOrWhiteSpace(FilePath))
            {
                FilePath = DefaultFilePath();
            }

            OnModeChanged();
            OnIntervalChanged();
        }

        public string FilePath { get; set; }

        public bool IsEnabled { get; set; }

        public FileLoggerMode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                if (_mode == value) return;

                _mode = value;
                OnModeChanged();
            }
        }

        public int FlushInterval
        {
            get
            {
                return _flushInterval;
            }
            set
            {
                if (_flushInterval == value) return;

                _flushInterval = value;
                OnIntervalChanged();
            }
        }

        public override void Log(LogBuilder log)
        {
            if (!IsEnabled) return;

            var str = log.Build();

            lock (_buffer)
            {
                if (_mode is FileLoggerMode.AppendToFile)
                {
                    File.AppendAllText(FilePath, str);
                }
                else if (_mode is FileLoggerMode.FlushBufferSize)
                {
                    _buffer.Add(str);

                    if (_buffer.Count >= _flushInterval)
                    {
                        Flush();
                    }
                }
                else
                {
                    _buffer.Add(str);
                }
            }

            base.Log(log);
        }

        public void Flush()
        {
            if (!IsEnabled) return;

            lock (_buffer)
            {
                File.AppendAllLines(FilePath, _buffer);
                _buffer.Clear();
            }
        }

        public void Enable() => IsEnabled = true;
        public void Disable() => IsEnabled = false;
        public void Toggle() => IsEnabled = !IsEnabled;

        private void OnIntervalChanged()
        {
            if (_timer != null)
            {
                _timer.Change(0, _flushInterval);
            }
        }

        private void OnModeChanged()
        {
            Flush();

            if (_timer != null && _mode != FileLoggerMode.FlushInterval)
            {
                _timer.Dispose();
            }

            if (_mode is FileLoggerMode.FlushInterval)
            {
                _timer = new Timer(TimerCallback, null, 0, _flushInterval);
            }
        }

        private void TimerCallback(object state)
        {
            Flush();
        }

        public static string DefaultFilePath() => $"{DirectoryManager.Roaming}/logs/{DefaultFileName()}";
        public static string DefaultFileName() => FileManager.FixFilePath($"Output Log - {DateTime.Now.ToString("G")}.txt");
    }
}