using helpers.Network.Features;

using System;
using System.Collections.Generic;
using System.Linq;

namespace helpers.Network.Callbacks
{
    public class NetworkCallbackManager : NetworkFeatureBase, INetworkCallbackManager
    {
        private readonly Dictionary<Type, Delegate> _singletonHandlers = new Dictionary<Type, Delegate>();
        private readonly Dictionary<Type, List<Delegate>> _multipleHandlers = new Dictionary<Type, List<Delegate>>();

        public int TotalHandlers => _multipleHandlers.Values.Sum(x => x.Count) + _singletonHandlers.Count;

        public bool Accepts(object data) => data != null;
        public bool Process(object data)
        {
            var type = data.GetType();
            bool anyExecuted = false;

            if (_singletonHandlers.TryGetValue(type, out var handler))
            {
                var handlerType = handler.GetType();
                if (handlerType == typeof(Func<,>))
                {
                    var returnValue = handler.DynamicInvoke(data);
                    if (handler != null)
                    {
                        this.ExecuteIfClient(client => client.Send(returnValue));
                        this.ExecuteIfServerPeer(peer => peer.Send(returnValue));
                    }
                    anyExecuted = true;
                }
                else
                {
                    handler.DynamicInvoke(data);
                    anyExecuted = true;
                }
            }

            if (_multipleHandlers.TryGetValue(type, out var handlers))
            {
                handlers.ForEach(handler =>
                {
                    var handlerType = handler.GetType();
                    if (handlerType == typeof(Func<,>))
                    {
                        var returnValue = handler.DynamicInvoke(data);
                        if (handler != null)
                        {
                            this.ExecuteIfClient(client => client.Send(returnValue));
                            this.ExecuteIfServerPeer(peer => peer.Send(returnValue));
                        }
                        anyExecuted = true;
                    }
                    else
                    {
                        handler.DynamicInvoke(data);
                        anyExecuted = true;
                    }
                });
            }

            return anyExecuted;
        }

        public IList<Delegate> GetHandlers<THandler>() => GetHandlers(typeof(THandler));
        public IList<Delegate> GetHandlers(Type type)
        {
            var handlerList = new List<Delegate>();

            if (_multipleHandlers.TryGetValue(type, out var handlers)) handlerList.AddRange(handlers);
            if (_singletonHandlers.ContainsKey(type)) handlerList.Add(_singletonHandlers[type]);

            return handlers;
        }

        public void AddHandler<THandler>(Action<THandler> handler)
        {
            if (!_multipleHandlers.ContainsKey(typeof(THandler))) _multipleHandlers.Add(typeof(THandler), new List<Delegate>());
            _multipleHandlers[typeof(THandler)].Add(handler);
        }

        public void AddHandler<THandler>(Func<THandler, object> handler)
        {
            if (!_multipleHandlers.ContainsKey(typeof(THandler))) _multipleHandlers.Add(typeof(THandler), new List<Delegate>());
            _multipleHandlers[typeof(THandler)].Add(handler);
        }

        public void RemoveHandler<THandler>(Action<THandler> handler)
        {
            if (!_multipleHandlers.ContainsKey(typeof(THandler))) _multipleHandlers.Add(typeof(THandler), new List<Delegate>());
            _multipleHandlers[typeof(THandler)].Remove(handler);
        }

        public void RemoveHandler<THandler>(Func<THandler, object> handler)
        {
            if (!_multipleHandlers.ContainsKey(typeof(THandler))) _multipleHandlers.Add(typeof(THandler), new List<Delegate>());
            _multipleHandlers[typeof(THandler)].Remove(handler);
        }

        public void ReplaceHandler<THandler>(Action<THandler> handler)
        {
            _singletonHandlers[typeof(THandler)] = handler;
        }

        public void ReplaceHandler<THandler>(Func<THandler, object> handler)
        {
            _singletonHandlers[typeof(THandler)] = handler;
        }
    }
}
