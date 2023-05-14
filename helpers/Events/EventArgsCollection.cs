using System;
using System.Collections.Generic;

namespace helpers.Events
{
    public class EventArgsCollection
    {
        private Dictionary<string, object> _args = new Dictionary<string, object>();

        public T Get<T>(string key) => _args[key].SafeAs<T>();

        public bool TryGet<T>(string key, out T value)
        {
            value = default;

            if (_args.TryGetValue(key, out object objValue))
            {
                if (objValue is T t)
                {
                    value = t;
                    return true;
                }
            }

            return false;
        }

        public void Modify<T>(string key, Action<T> modify)
        {
            if (TryGet<T>(key, out var value))
            {
                modify?.Invoke(value);
            }
        }

        public EventArgsCollection WithArg(string argKey, object argValue)
        {
            _args[argKey] = argValue;
            return this;
        }

        public void Clear()
        {
            _args.Clear();
        }
    }
}