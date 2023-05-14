using System;
using System.Collections.Generic;

namespace helpers.Events
{
    public class EventProvider
    {
        private List<Action<EventArgsCollection>> _handlers = new List<Action<EventArgsCollection>>();

        public void Invoke(params KeyValuePair<string, object>[] args)
        {
            var evArgs = new EventArgsCollection();

            foreach (var arg in args)
            {
                evArgs.WithArg(arg.Key, arg.Value);
            }

            Invoke(evArgs);
        }

        public void Invoke(EventArgsCollection eventArgsCollection)
        {
            _handlers.ForEach(x =>
            {
                ExceptionManager.ExecuteSafe(() =>
                {
                    x?.Invoke(eventArgsCollection);
                });
            });
        }

        public void Add(Action<EventArgsCollection> handler)
        {
            if (Has(handler))
            {
                return;
            }

            _handlers.Add(handler);
        }

        public void Remove(Action<EventArgsCollection> handler)
        {
            if (!Has(handler))
            {
                return;
            }

            _handlers.Remove(handler);
        }

        public bool Has(Action<EventArgsCollection> handler)
        {
            return _handlers.Contains(handler);
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }
}