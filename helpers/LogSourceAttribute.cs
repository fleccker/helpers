using System;

namespace helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class LogSourceAttribute : Attribute
    {
        private string _source;

        public LogSourceAttribute(object source)
        {
            _source = source?.ToString() ?? "unknown";
        }

        public string GetSource()
            => _source;

        public void SetSource(string source)
            => _source = source;
    }
}