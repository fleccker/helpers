﻿using helpers.Extensions;
using helpers.Parsers.Log;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace helpers.Logging
{
    public class LogBuilder 
    {
        private ConcurrentQueue<Tuple<ConsoleColor, string>> _log;

        public const int ColorStrLength = 5;

        public bool IsValid { get => _log != null; }

        public ConsoleColor DefaultColor { get; set; } = ConsoleColor.White;

        public ConsoleColor? LastColor { get; private set; }

        public string LastLog { get; private set; }

        public LogBuilder()
        {
            _log = new ConcurrentQueue<Tuple<ConsoleColor, string>>();
        }

        public LogBuilder(IEnumerable<Tuple<ConsoleColor, string>> log)
        {
            _log = new ConcurrentQueue<Tuple<ConsoleColor, string>>();
            _log.EnqueueMany(log);
        }

        public bool TryGetNext(out Tuple<ConsoleColor, string> next)
        {
            if (!IsValid)
            {
                next = null;
                return false;
            }

            return _log.TryDequeue(out next);
        }

        public LogBuilder WithLine(object line, ConsoleColor? color = null)
        {
            if (color is null || !color.HasValue)
                color = DefaultColor;

            if (_log is null)
                _log = new ConcurrentQueue<Tuple<ConsoleColor, string>>();

            var strLine = line?.ToString();

            if (string.IsNullOrWhiteSpace(strLine))
                strLine = "empty";

            if (strLine.HasHtmlTags(out var openIndexes, out var closeIndexes) && LogParser.TryParse(strLine, out var tags))
            {
                tags.ForEach(tag =>
                {
                    var color = ConsoleColor.Green;

                    Enum.TryParse(tag.Value, true, out color);

                    _log.Enqueue(new Tuple<ConsoleColor, string>(color, tag.StringValue));
                });

                return this;
            }

            _log.Enqueue(new Tuple<ConsoleColor, string>(color.Value, strLine));
            return this;
        }

        public void Reset()
        {
            _log.Clear();
        }
    }
}