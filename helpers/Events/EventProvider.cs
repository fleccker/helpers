using System;
using System.Collections.Generic;

namespace helpers.Events
{
    [LogSource("Event Provider")]
    public class EventProvider
    {
        private List<Action<EventArgsCollection>> _handlers = new List<Action<EventArgsCollection>>();
        private List<Action> _parameterLessHandlers = new List<Action>();

        public void Invoke(params KeyValuePair<string, object>[] args)
        {
            var evArgs = new EventArgsCollection();
            foreach (var arg in args) evArgs.WithArg(arg.Key, arg.Value);
            Invoke(evArgs);
        }

        public void Invoke(EventArgsCollection eventArgsCollection)
        {
            _handlers.ForEach(y => y.Invoke(eventArgsCollection));
            _parameterLessHandlers.ForEach(x => x.Invoke());
        }

        public void Add(Action<EventArgsCollection> handler)
        {
            if (Has(handler)) return;
            _handlers.Add(handler);
        }

        public void Add(Action handler)
        {
            if (Has(handler)) return;
            _parameterLessHandlers.Add(handler);
        }

        public void Remove(Action<EventArgsCollection> handler)
        {
            if (!Has(handler)) return;
            _handlers.Remove(handler);
        }

        public void Remove(Action handler)
        {
            if (!Has(handler)) return;
            _parameterLessHandlers.Remove(handler);
        }

        public bool Has(Action<EventArgsCollection> handler) => _handlers.Contains(handler);
        public bool Has(Action action) => _parameterLessHandlers.Contains(action);
        public void Clear()
        {
            _handlers.Clear();
            _parameterLessHandlers.Clear();
        }
    }
}