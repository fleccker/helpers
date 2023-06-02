using helpers.Extensions;
using helpers.Random;

using System;
using System.Collections.Generic;

namespace helpers.Events
{
    [LogSource("Event Provider")]
    public class EventProvider
    {
        public string Id { get; }
        
        public EventProvider() 
        { 
            Id = RandomGeneration.Default.GetString(5);
            EventManager.Add(this);
        }

        public EventProvider(string id)
        {
            Id = id;
            EventManager.Add(this);
        }
        
        private List<Action<ObjectCollection>> _handlers = new List<Action<ObjectCollection>>();
        private List<Action> _parameterLessHandlers = new List<Action>();

        public void Invoke(params object[] args)
        {
            if (args is null || !args.Any())
            {
                Invoke((ObjectCollection)null);
                return;
            }

            var collection = new ObjectCollection();

            foreach (var arg in args)
            {
                if (arg is KeyValuePair<string, object> valuePair) collection.Add(valuePair.Value, valuePair.Key);
                else if (arg is Tuple<string, object> tuple) collection.Add(tuple.Item2, tuple.Item1);
                else collection.Add(arg);
            }

            Invoke(collection);
        }

        public void Invoke(ObjectCollection eventArgsCollection)
        {
            if (eventArgsCollection != null) _handlers.ForEach(y => y.Invoke(eventArgsCollection));
            _parameterLessHandlers.ForEach(x => x.Invoke());
        }

        public void Add(Action<ObjectCollection> handler)
        {
            if (Has(handler)) return;
            _handlers.Add(handler);
        }

        public void Add(Action handler)
        {
            if (Has(handler)) return;
            _parameterLessHandlers.Add(handler);
        }

        public void Remove(Action<ObjectCollection> handler)
        {
            if (!Has(handler)) return;
            _handlers.Remove(handler);
        }

        public void Remove(Action handler)
        {
            if (!Has(handler)) return;
            _parameterLessHandlers.Remove(handler);
        }

        public bool Has(Action<ObjectCollection> handler) => _handlers.Contains(handler);
        public bool Has(Action action) => _parameterLessHandlers.Contains(action);

        public void Clear()
        {
            _handlers.Clear();
            _parameterLessHandlers.Clear();
        }
    }
}