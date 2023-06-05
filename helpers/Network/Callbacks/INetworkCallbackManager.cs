using helpers.Network.Data;
using helpers.Network.Features;

using System;
using System.Collections.Generic;

namespace helpers.Network.Callbacks
{
    public interface INetworkCallbackManager : INetworkFeature, IDataTarget
    {
        int TotalHandlers { get; }

        void AddHandler<THandler>(Action<THandler> handler);
        void AddHandler<THandler>(Func<THandler, object> handler);

        void ReplaceHandler<THandler>(Action<THandler> handler);
        void ReplaceHandler<THandler>(Func<THandler, object> handler);

        void RemoveHandler<THandler>(Action<THandler> handler);
        void RemoveHandler<THandler>(Func<THandler, object> handler);

        IList<Delegate> GetHandlers(Type type);
        IList<Delegate> GetHandlers<THandler>();
    }
}