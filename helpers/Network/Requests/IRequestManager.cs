using helpers.Network.Data;
using helpers.Network.Features;

using System;

namespace helpers.Network.Requests
{
    public interface IRequestManager : INetworkFeature, IDataTarget
    {
        int Waiting { get; }
        int Sent { get; }
        int Received { get; }

        void Send(IResponse response, IRequest request);
        void Send(object response, IRequest request, bool isSuccess = true);
        void Send(IRequest request, Action<IResponse> callback);
        void Send(object request, Action<IResponse> callback);

        void AddHandler<TRequest>(Action<IRequest, TRequest> handler);
        void AddHandler<TRequest, TResponse>(Func<IRequest, TRequest, TResponse> handler);

        void RemoveHandler<TRequest>(Action<IRequest, TRequest> handler);
        void RemoveHandler<TRequest, TResponse>(Func<IRequest, TRequest, TResponse> handler);

        void ReplaceHandler<TRequest>(Action<IRequest, TRequest> handler);
        void ReplaceHandler<TRequest, TResponse>(Func<IRequest, TRequest, TResponse> handler);
    }
}